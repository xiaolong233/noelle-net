using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NoelleNet.Validation;

namespace NoelleNet.AspNetCore.ExceptionHandling;

/// <summary>
/// <see cref="NoelleExceptionHandlingMiddleware"/> 的契约测试：
/// 异常拦截写入、写入失败重抛（与 IExceptionHandler 的容错策略差异）与日志级别决策
/// </summary>
public class NoelleExceptionHandlingMiddlewareTests
{
    private readonly Mock<ILogger<NoelleExceptionHandlingMiddleware>> _loggerMock;
    private readonly Mock<IErrorResponseWriter> _writerMock;

    public NoelleExceptionHandlingMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<NoelleExceptionHandlingMiddleware>>();
        // Moq 松散模拟的 IsEnabled 默认返回 false，会让中间件直接跳过日志调用，导致级别断言失去意义
        _loggerMock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _writerMock = new Mock<IErrorResponseWriter>();
        _writerMock
            .Setup(w => w.TryWriteAsync(It.IsAny<HttpContext>(), It.IsAny<Exception>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private NoelleExceptionHandlingMiddleware CreateMiddleware(RequestDelegate next) =>
        new(next, _loggerMock.Object, _writerMock.Object);

    /// <summary>
    /// 无异常时不写入、不记录日志
    /// </summary>
    [Fact]
    public async Task InvokeAsync_NoException_ShouldNotWrite()
    {
        var middleware = CreateMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(new DefaultHttpContext());

        _writerMock.Verify(
            w => w.TryWriteAsync(It.IsAny<HttpContext>(), It.IsAny<Exception>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _loggerMock.Verify(
            l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    /// <summary>
    /// 下游抛异常时应写入并吞掉异常
    /// </summary>
    [Fact]
    public async Task InvokeAsync_NextThrows_ShouldWriteAndSwallow()
    {
        var exception = new InvalidOperationException("boom");
        var httpContext = new DefaultHttpContext();

        await CreateMiddleware(_ => throw exception).InvokeAsync(httpContext);

        _writerMock.Verify(w => w.TryWriteAsync(httpContext, exception, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// 写入器抛异常时向上重抛（Middleware 无兜底处理器，与 IExceptionHandler 策略不同）
    /// </summary>
    [Fact]
    public async Task InvokeAsync_WriterThrows_ShouldRethrow()
    {
        _writerMock
            .Setup(w => w.TryWriteAsync(It.IsAny<HttpContext>(), It.IsAny<Exception>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("write failed"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateMiddleware(_ => throw new Exception("boom")).InvokeAsync(new DefaultHttpContext()));
    }

    /// <summary>
    /// 日志级别决策：IHasLogLevel 优先，普通异常按 Error 记录
    /// </summary>
    [Fact]
    public async Task InvokeAsync_ShouldLogWithDecidedLevel()
    {
        await CreateMiddleware(_ => throw new NoelleValidationException([]) { LogLevel = LogLevel.Critical })
            .InvokeAsync(new DefaultHttpContext());
        _loggerMock.Verify(
            l => l.Log(LogLevel.Critical, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        await CreateMiddleware(_ => throw new Exception("generic")).InvokeAsync(new DefaultHttpContext());
        _loggerMock.Verify(
            l => l.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// 实体未找到是可预期的客户端错误（404），应经 IHasLogLevel 以 Information 级别记录，
    /// 而不是落回普通异常的 Error 兜底分支
    /// </summary>
    [Fact]
    public async Task InvokeAsync_EntityNotFoundException_ShouldLogWithInformation()
    {
        await CreateMiddleware(_ => throw new EntityNotFoundException<string>(42))
            .InvokeAsync(new DefaultHttpContext());

        _loggerMock.Verify(
            l => l.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// 日志级别决策表：异常显式声明优先；未声明时按内置类型表映射；未匹配的一律 Error。
    /// 用例见 <see cref="ExceptionLogLevelCases"/>，与 Handler / Filter 共用同一份。
    /// </summary>
    [Theory]
    [MemberData(nameof(ExceptionLogLevelCases.All), MemberType = typeof(ExceptionLogLevelCases))]
    public async Task InvokeAsync_ShouldResolveLogLevelByException(string caseName, LogLevel expected)
    {
        await CreateMiddleware(_ => throw ExceptionLogLevelCases.Create(caseName))
            .InvokeAsync(new DefaultHttpContext());

        _loggerMock.Verify(
            l => l.Log(expected, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
