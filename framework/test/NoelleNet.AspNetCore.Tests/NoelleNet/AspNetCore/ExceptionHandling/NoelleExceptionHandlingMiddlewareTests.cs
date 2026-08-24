using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NoelleNet.Validation;

namespace NoelleNet.AspNetCore.ExceptionHandling;

public class NoelleExceptionHandlingMiddlewareTests
{
    private readonly Mock<ILogger<NoelleExceptionHandlingMiddleware>> _loggerMock;
    private readonly Mock<IErrorResponseWriter> _writerMock;

    public NoelleExceptionHandlingMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<NoelleExceptionHandlingMiddleware>>();
        _writerMock = new Mock<IErrorResponseWriter>();
        _writerMock
            .Setup(w => w.TryWriteAsync(It.IsAny<HttpContext>(), It.IsAny<Exception>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private NoelleExceptionHandlingMiddleware CreateMiddleware(RequestDelegate next) =>
        new(next, _loggerMock.Object, _writerMock.Object);

    [Fact]
    public void Constructor_NullNext_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new NoelleExceptionHandlingMiddleware(null!, _loggerMock.Object, _writerMock.Object));
    }

    [Fact]
    public void Constructor_NullLogger_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new NoelleExceptionHandlingMiddleware(_ => Task.CompletedTask, null!, _writerMock.Object));
    }

    [Fact]
    public void Constructor_NullErrorResponseWriter_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new NoelleExceptionHandlingMiddleware(_ => Task.CompletedTask, _loggerMock.Object, null!));
    }

    [Fact]
    public async Task InvokeAsync_NoException_ShouldNotWrite()
    {
        bool nextInvoked = false;
        var middleware = CreateMiddleware(_ =>
        {
            nextInvoked = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(new DefaultHttpContext());

        Assert.True(nextInvoked);
        _writerMock.Verify(
            w => w.TryWriteAsync(It.IsAny<HttpContext>(), It.IsAny<Exception>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _loggerMock.Verify(
            l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_NextThrows_ShouldWriteAndSwallow()
    {
        var exception = new InvalidOperationException("boom");
        var httpContext = new DefaultHttpContext();
        var middleware = CreateMiddleware(_ => throw exception);

        await middleware.InvokeAsync(httpContext);

        _writerMock.Verify(
            w => w.TryWriteAsync(httpContext, exception, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WriterThrows_ShouldRethrow()
    {
        _writerMock
            .Setup(w => w.TryWriteAsync(It.IsAny<HttpContext>(), It.IsAny<Exception>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("write failed"));

        var middleware = CreateMiddleware(_ => throw new Exception("boom"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => middleware.InvokeAsync(new DefaultHttpContext()));

        _loggerMock.Verify(
            l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_GenericException_ShouldLogError()
    {
        var middleware = CreateMiddleware(_ => throw new Exception("generic error"));

        await middleware.InvokeAsync(new DefaultHttpContext());

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_IHasLogLevel_ShouldUseCustomLogLevel()
    {
        var middleware = CreateMiddleware(_ => throw new NoelleValidationException([]) { LogLevel = LogLevel.Critical });

        await middleware.InvokeAsync(new DefaultHttpContext());

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
