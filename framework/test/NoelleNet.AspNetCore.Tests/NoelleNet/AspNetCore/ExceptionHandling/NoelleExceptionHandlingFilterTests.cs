using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NoelleNet.Http;
using NoelleNet.Logging;
using NoelleNet.Validation;
using System.Net;

namespace NoelleNet.AspNetCore.ExceptionHandling;

public class NoelleExceptionHandlingFilterTests
{
    private readonly Mock<ILogger<NoelleExceptionHandlingFilter>> _loggerMock;
    private readonly Mock<IExceptionToErrorConverter> _converterMock;
    private readonly Mock<IHttpExceptionStatusCodeFinder> _finderMock;
    private readonly Mock<IOptions<NoelleExceptionHandlingOptions>> _optionsMock;
    private readonly NoelleExceptionHandlingFilter _filter;

    public NoelleExceptionHandlingFilterTests()
    {
        _loggerMock = new Mock<ILogger<NoelleExceptionHandlingFilter>>();
        _converterMock = new Mock<IExceptionToErrorConverter>();
        _finderMock = new Mock<IHttpExceptionStatusCodeFinder>();
        _optionsMock = new Mock<IOptions<NoelleExceptionHandlingOptions>>();
        _optionsMock.Setup(o => o.Value).Returns(new NoelleExceptionHandlingOptions());

        _converterMock
            .Setup(c => c.Convert(It.IsAny<Exception>(), It.IsAny<Action<NoelleExceptionHandlingOptions>?>()))
            .Returns(new RemoteCallErrorInfo("test error"));
        _finderMock
            .Setup(f => f.GetStatusCode(It.IsAny<HttpContext>(), It.IsAny<Exception>()))
            .Returns(HttpStatusCode.InternalServerError);

        _filter = new NoelleExceptionHandlingFilter(
            _loggerMock.Object,
            _converterMock.Object,
            _finderMock.Object,
            _optionsMock.Object);
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
            new NoelleExceptionHandlingFilter(null!, _converterMock.Object, _finderMock.Object, _optionsMock.Object));
    }

    [Fact]
    public void Constructor_NullConverter_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new NoelleExceptionHandlingFilter(_loggerMock.Object, null!, _finderMock.Object, _optionsMock.Object));
    }

    [Fact]
    public void Constructor_NullFinder_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new NoelleExceptionHandlingFilter(_loggerMock.Object, _converterMock.Object, null!, _optionsMock.Object));
    }

    [Fact]
    public void Constructor_NullOptions_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new NoelleExceptionHandlingFilter(_loggerMock.Object, _converterMock.Object, _finderMock.Object, null!));
    }

    [Fact]
    public async Task OnExceptionAsync_ExceptionAlreadyHandled_ShouldSkip()
    {
        var context = CreateExceptionContext(new Exception("test"));
        context.ExceptionHandled = true;

        await _filter.OnExceptionAsync(context);

        Assert.True(context.ExceptionHandled);
        Assert.Null(context.Result);
        _loggerMock.Verify(
            l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task OnExceptionAsync_ShouldSetJsonResult()
    {
        var exception = new Exception("test error");
        var context = CreateExceptionContext(exception);

        await _filter.OnExceptionAsync(context);

        Assert.True(context.ExceptionHandled);
        Assert.NotNull(context.Result);
        var jsonResult = Assert.IsType<JsonResult>(context.Result);
        Assert.NotNull(jsonResult.Value);
        Assert.IsType<ErrorResponseDto>(jsonResult.Value);
    }

    [Fact]
    public async Task OnExceptionAsync_ShouldUseStatusCodeFromFinder()
    {
        var exception = new Exception("test");
        var context = CreateExceptionContext(exception);
        _finderMock.Setup(f => f.GetStatusCode(It.IsAny<HttpContext>(), exception)).Returns(HttpStatusCode.BadRequest);

        await _filter.OnExceptionAsync(context);

        var jsonResult = Assert.IsType<JsonResult>(context.Result);
        Assert.Equal((int)HttpStatusCode.BadRequest, jsonResult.StatusCode);
    }

    [Fact]
    public async Task OnExceptionAsync_BusinessExceptionLogLevelWarning_ShouldLogWarning()
    {
        // BusinessException implements IHasLogLevel with default LogLevel.Warning,
        // so the switch hits IHasLogLevel case first, not IBusinessException
        var exception = new BusinessException("ERR", "business error");
        var context = CreateExceptionContext(exception);
        _finderMock.Setup(f => f.GetStatusCode(It.IsAny<HttpContext>(), exception)).Returns(HttpStatusCode.Forbidden);

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
        var exception = new Exception("generic error");
        var context = CreateExceptionContext(exception);

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
    public async Task OnExceptionAsync_ShouldPassExceptionToConverter()
    {
        var exception = new InvalidOperationException("invalid op");
        var context = CreateExceptionContext(exception);

        await _filter.OnExceptionAsync(context);

        _converterMock.Verify(
            c => c.Convert(exception, It.IsAny<Action<NoelleExceptionHandlingOptions>?>()),
            Times.Once);
    }

    [Fact]
    public async Task OnExceptionAsync_Response_ShouldBeErrorResponseDto()
    {
        var error = new RemoteCallErrorInfo("custom error message", code: "ERR001");
        _converterMock
            .Setup(c => c.Convert(It.IsAny<Exception>(), It.IsAny<Action<NoelleExceptionHandlingOptions>?>()))
            .Returns(error);

        var exception = new Exception("test");
        var context = CreateExceptionContext(exception);

        await _filter.OnExceptionAsync(context);

        var jsonResult = Assert.IsType<JsonResult>(context.Result);
        var response = Assert.IsType<ErrorResponseDto>(jsonResult.Value);
        Assert.Equal("custom error message", response.Error.Message);
        Assert.Equal("ERR001", response.Error.Code);
    }
}
