using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NoelleNet.Validation;

namespace NoelleNet.AspNetCore.ExceptionHandling;

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

    private static HttpContext CreateHttpContext() => new DefaultHttpContext();

    [Fact]
    public void Constructor_NullLogger_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new NoelleExceptionHandler(null!, _writerMock.Object));
    }

    [Fact]
    public void Constructor_NullErrorResponseWriter_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new NoelleExceptionHandler(_loggerMock.Object, null!));
    }

    [Fact]
    public async Task TryHandleAsync_WriterReturnsTrue_ShouldReturnTrue()
    {
        var exception = new Exception("test");

        bool result = await _handler.TryHandleAsync(CreateHttpContext(), exception, CancellationToken.None);

        Assert.True(result);
    }

    [Fact]
    public async Task TryHandleAsync_WriterReturnsFalse_ShouldReturnFalse()
    {
        _writerMock
            .Setup(w => w.TryWriteAsync(It.IsAny<HttpContext>(), It.IsAny<Exception>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        bool result = await _handler.TryHandleAsync(CreateHttpContext(), new Exception("test"), CancellationToken.None);

        Assert.False(result);
    }

    [Fact]
    public async Task TryHandleAsync_ShouldPassExceptionToWriter()
    {
        var httpContext = CreateHttpContext();
        var exception = new InvalidOperationException("invalid op");

        await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        _writerMock.Verify(
            w => w.TryWriteAsync(httpContext, exception, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_WriterThrows_ShouldNotRethrow()
    {
        _writerMock
            .Setup(w => w.TryWriteAsync(It.IsAny<HttpContext>(), It.IsAny<Exception>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("write failed"));

        bool result = await _handler.TryHandleAsync(CreateHttpContext(), new Exception("test"), CancellationToken.None);

        Assert.False(result);
        _loggerMock.Verify(
            l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_IHasLogLevel_ShouldUseCustomLogLevel()
    {
        var exception = new NoelleValidationException([]) { LogLevel = LogLevel.Critical };

        await _handler.TryHandleAsync(CreateHttpContext(), exception, CancellationToken.None);

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_GenericException_ShouldLogError()
    {
        await _handler.TryHandleAsync(CreateHttpContext(), new Exception("generic error"), CancellationToken.None);

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
