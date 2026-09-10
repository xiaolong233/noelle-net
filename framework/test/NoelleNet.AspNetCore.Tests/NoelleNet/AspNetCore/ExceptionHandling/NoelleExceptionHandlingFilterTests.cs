using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using NoelleNet.Validation;

namespace NoelleNet.AspNetCore.ExceptionHandling;

/// <summary>
/// <see cref="NoelleExceptionHandlingFilter"/> 的契约测试：ExceptionHandled 标记、写入失败重抛与日志级别决策
/// </summary>
public class NoelleExceptionHandlingFilterTests
{
    private readonly Mock<ILogger<NoelleExceptionHandlingFilter>> _loggerMock;
    private readonly Mock<IErrorResponseWriter> _writerMock;
    private readonly NoelleExceptionHandlingFilter _filter;

    public NoelleExceptionHandlingFilterTests()
    {
        _loggerMock = new Mock<ILogger<NoelleExceptionHandlingFilter>>();
        // Moq 松散模拟的 IsEnabled 默认返回 false，会让筛选器直接跳过日志调用，导致级别断言失去意义
        _loggerMock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _writerMock = new Mock<IErrorResponseWriter>();
        _writerMock
            .Setup(w => w.TryWriteAsync(It.IsAny<HttpContext>(), It.IsAny<Exception>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _filter = new NoelleExceptionHandlingFilter(_loggerMock.Object, _writerMock.Object);
    }

    private static ExceptionContext CreateExceptionContext(Exception exception, bool handled = false)
    {
        return new ExceptionContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
            [])
        {
            Exception = exception,
            ExceptionHandled = handled
        };
    }

    /// <summary>
    /// 已被其他处理器标记为 handled 时应跳过，不写入不记录
    /// </summary>
    [Fact]
    public async Task OnExceptionAsync_AlreadyHandled_ShouldSkip()
    {
        var context = CreateExceptionContext(new Exception("test"), handled: true);

        await _filter.OnExceptionAsync(context);

        _writerMock.Verify(
            w => w.TryWriteAsync(It.IsAny<HttpContext>(), It.IsAny<Exception>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _loggerMock.Verify(
            l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    /// <summary>
    /// 写入成功/失败应分别设置/不设置 ExceptionHandled，并传递原始异常
    /// </summary>
    [Fact]
    public async Task OnExceptionAsync_ShouldSetHandledByWriterResult()
    {
        var exception = new InvalidOperationException("invalid op");
        var context = CreateExceptionContext(exception);

        await _filter.OnExceptionAsync(context);

        Assert.True(context.ExceptionHandled);
        _writerMock.Verify(w => w.TryWriteAsync(context.HttpContext, exception, It.IsAny<CancellationToken>()), Times.Once);

        _writerMock
            .Setup(w => w.TryWriteAsync(It.IsAny<HttpContext>(), It.IsAny<Exception>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var context2 = CreateExceptionContext(exception);
        await _filter.OnExceptionAsync(context2);
        Assert.False(context2.ExceptionHandled);
    }

    /// <summary>
    /// 写入器抛异常时应向上重抛（MVC 管道无兜底）
    /// </summary>
    [Fact]
    public async Task OnExceptionAsync_WriterThrows_ShouldRethrow()
    {
        _writerMock
            .Setup(w => w.TryWriteAsync(It.IsAny<HttpContext>(), It.IsAny<Exception>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("write failed"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _filter.OnExceptionAsync(CreateExceptionContext(new Exception("test"))));
    }

    /// <summary>
    /// 日志级别决策：IHasLogLevel 优先，普通异常按 Error 记录
    /// </summary>
    [Fact]
    public async Task OnExceptionAsync_ShouldLogWithDecidedLevel()
    {
        await _filter.OnExceptionAsync(CreateExceptionContext(new NoelleValidationException([]) { LogLevel = LogLevel.Critical }));
        _loggerMock.Verify(
            l => l.Log(LogLevel.Critical, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        await _filter.OnExceptionAsync(CreateExceptionContext(new Exception("generic")));
        _loggerMock.Verify(
            l => l.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// 实体未找到是可预期的客户端错误（404），应经 IHasLogLevel 以 Information 级别记录，
    /// 而不是落回普通异常的 Error 兜底分支
    /// </summary>
    [Fact]
    public async Task OnExceptionAsync_EntityNotFoundException_ShouldLogWithInformation()
    {
        await _filter.OnExceptionAsync(CreateExceptionContext(new EntityNotFoundException<string>(42)));

        _loggerMock.Verify(
            l => l.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// 日志级别决策表：异常显式声明优先；未声明时按内置类型表映射；未匹配的一律 Error。
    /// 用例见 <see cref="ExceptionLogLevelCases"/>，与 Handler / Middleware 共用同一份。
    /// </summary>
    [Theory]
    [MemberData(nameof(ExceptionLogLevelCases.All), MemberType = typeof(ExceptionLogLevelCases))]
    public async Task OnExceptionAsync_ShouldResolveLogLevelByException(string caseName, LogLevel expected)
    {
        await _filter.OnExceptionAsync(CreateExceptionContext(ExceptionLogLevelCases.Create(caseName)));

        _loggerMock.Verify(
            l => l.Log(expected, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
