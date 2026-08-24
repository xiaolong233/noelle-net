using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using NoelleNet.Validation;

namespace NoelleNet.AspNetCore.ExceptionHandling;

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

    private static ExceptionContext CreateExceptionContext(Exception exception)
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor());

        return new ExceptionContext(actionContext, [])
        {
            Exception = exception
        };
    }

    [Fact]
    public void Constructor_NullLogger_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new NoelleExceptionHandlingFilter(null!, _writerMock.Object));
    }

    [Fact]
    public void Constructor_NullErrorResponseWriter_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new NoelleExceptionHandlingFilter(_loggerMock.Object, null!));
    }

    [Fact]
    public async Task OnExceptionAsync_ExceptionAlreadyHandled_ShouldSkip()
    {
        var context = CreateExceptionContext(new Exception("test"));
        context.ExceptionHandled = true;

        await _filter.OnExceptionAsync(context);

        Assert.True(context.ExceptionHandled);
        _writerMock.Verify(
            w => w.TryWriteAsync(It.IsAny<HttpContext>(), It.IsAny<Exception>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _loggerMock.Verify(
            l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task OnExceptionAsync_WriterReturnsTrue_ShouldMarkHandled()
    {
        var context = CreateExceptionContext(new Exception("test"));

        await _filter.OnExceptionAsync(context);

        Assert.True(context.ExceptionHandled);
    }

    [Fact]
    public async Task OnExceptionAsync_WriterReturnsFalse_ShouldNotMarkHandled()
    {
        _writerMock
            .Setup(w => w.TryWriteAsync(It.IsAny<HttpContext>(), It.IsAny<Exception>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var context = CreateExceptionContext(new Exception("test"));

        await _filter.OnExceptionAsync(context);

        Assert.False(context.ExceptionHandled);
    }

    [Fact]
    public async Task OnExceptionAsync_ShouldPassExceptionToWriter()
    {
        var exception = new InvalidOperationException("invalid op");
        var context = CreateExceptionContext(exception);

        await _filter.OnExceptionAsync(context);

        _writerMock.Verify(
            w => w.TryWriteAsync(context.HttpContext, exception, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task OnExceptionAsync_WriterThrows_ShouldRethrow()
    {
        _writerMock
            .Setup(w => w.TryWriteAsync(It.IsAny<HttpContext>(), It.IsAny<Exception>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("write failed"));

        var context = CreateExceptionContext(new Exception("test"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _filter.OnExceptionAsync(context));

        Assert.False(context.ExceptionHandled);
        _loggerMock.Verify(
            l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task OnExceptionAsync_IHasLogLevel_ShouldUseCustomLogLevel()
    {
        var exception = new NoelleValidationException([]) { LogLevel = LogLevel.Critical };
        var context = CreateExceptionContext(exception);

        await _filter.OnExceptionAsync(context);

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
    public async Task OnExceptionAsync_GenericException_ShouldLogError()
    {
        var context = CreateExceptionContext(new Exception("generic error"));

        await _filter.OnExceptionAsync(context);

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
    public async Task OnExceptionAsync_OperationCanceledException_ShouldLogWarning()
    {
        var context = CreateExceptionContext(new OperationCanceledException());

        await _filter.OnExceptionAsync(context);

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task OnExceptionAsync_TaskCanceledException_ShouldLogDebug()
    {
        var context = CreateExceptionContext(new TaskCanceledException());

        await _filter.OnExceptionAsync(context);

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
