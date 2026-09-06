using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;

namespace NoelleNet.AspNetCore.ExceptionHandling;

/// <summary>
/// <see cref="NoelleExceptionHandlingFilter"/> 的契约测试：ExceptionHandled 标记、写入失败重抛
/// </summary>
public class NoelleExceptionHandlingFilterTests
{
    private readonly Mock<ILogger<NoelleExceptionHandlingFilter>> _loggerMock;
    private readonly Mock<IErrorResponseWriter> _writerMock;
    private readonly NoelleExceptionHandlingFilter _filter;

    public NoelleExceptionHandlingFilterTests()
    {
        _loggerMock = new Mock<ILogger<NoelleExceptionHandlingFilter>>();
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
}
