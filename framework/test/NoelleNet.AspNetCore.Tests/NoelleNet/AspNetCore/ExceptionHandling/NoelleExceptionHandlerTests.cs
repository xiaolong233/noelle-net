using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NoelleNet.Validation;

namespace NoelleNet.AspNetCore.ExceptionHandling;

/// <summary>
/// <see cref="NoelleExceptionHandler"/> 的契约测试：写入结果透传、写入失败容错与日志级别决策
/// </summary>
public class NoelleExceptionHandlerTests
{
    private readonly Mock<ILogger<NoelleExceptionHandler>> _loggerMock;
    private readonly Mock<IErrorResponseWriter> _writerMock;
    private readonly NoelleExceptionHandler _handler;

    public NoelleExceptionHandlerTests()
    {
        _loggerMock = new Mock<ILogger<NoelleExceptionHandler>>();
        _writerMock = new Mock<IErrorResponseWriter>();
        _writerMock
            .Setup(w => w.TryWriteAsync(It.IsAny<HttpContext>(), It.IsAny<Exception>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _handler = new NoelleExceptionHandler(_loggerMock.Object, _writerMock.Object);
    }

    /// <summary>
    /// 写入结果应透传，并原样传递 HttpContext 与异常
    /// </summary>
    [Fact]
    public async Task TryHandleAsync_ShouldForwardResultAndPassContext()
    {
        var httpContext = new DefaultHttpContext();
        var exception = new InvalidOperationException("invalid op");

        Assert.True(await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None));
        _writerMock.Verify(w => w.TryWriteAsync(httpContext, exception, It.IsAny<CancellationToken>()), Times.Once);

        _writerMock
            .Setup(w => w.TryWriteAsync(It.IsAny<HttpContext>(), It.IsAny<Exception>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        Assert.False(await _handler.TryHandleAsync(new DefaultHttpContext(), exception, CancellationToken.None));
    }

    /// <summary>
    /// 写入器抛异常时应返回 false 交给其他处理器兜底（容错契约，与 Middleware 的重抛策略不同）
    /// </summary>
    [Fact]
    public async Task TryHandleAsync_WriterThrows_ShouldReturnFalseAndLog()
    {
        _writerMock
            .Setup(w => w.TryWriteAsync(It.IsAny<HttpContext>(), It.IsAny<Exception>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("write failed"));

        var result = await _handler.TryHandleAsync(new DefaultHttpContext(), new Exception("test"), CancellationToken.None);

        Assert.False(result);
        _loggerMock.Verify(
            l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// 日志级别决策：IHasLogLevel 优先，普通异常按 Error 记录
    /// </summary>
    [Fact]
    public async Task TryHandleAsync_ShouldLogWithDecidedLevel()
    {
        await _handler.TryHandleAsync(new DefaultHttpContext(), new NoelleValidationException([]) { LogLevel = LogLevel.Critical }, CancellationToken.None);
        _loggerMock.Verify(
            l => l.Log(LogLevel.Critical, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        await _handler.TryHandleAsync(new DefaultHttpContext(), new Exception("generic"), CancellationToken.None);
        _loggerMock.Verify(
            l => l.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
