using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Moq;
using NoelleNet.AspNetCore.ExceptionHandling.Localization;
using System.Net;

namespace NoelleNet.AspNetCore.Authorization;

public class NoelleAuthorizationErrorResponseMiddlewareTests
{
    [Fact]
    public void Constructor_NullNext_ShouldThrowArgumentNullException()
    {
        var localizerMock = new Mock<IStringLocalizer<NoelleExceptionHandlingResource>>();

        Assert.Throws<ArgumentNullException>(() =>
            new NoelleAuthorizationErrorResponseMiddleware(null!, localizerMock.Object));
    }

    [Fact]
    public void Constructor_NullLocalizer_ShouldThrowArgumentNullException()
    {
        var next = (RequestDelegate)(_ => Task.CompletedTask);

        Assert.Throws<ArgumentNullException>(() =>
            new NoelleAuthorizationErrorResponseMiddleware(next, null!));
    }

    [Fact]
    public async Task InvokeAsync_ResponseHasStarted_ShouldNotModifyResponse()
    {
        var next = (RequestDelegate)(_ => Task.CompletedTask);
        var localizerMock = new Mock<IStringLocalizer<NoelleExceptionHandlingResource>>();

        var responseMock = new Mock<HttpResponse>();
        responseMock.SetupGet(r => r.HasStarted).Returns(true);
        responseMock.SetupProperty(r => r.StatusCode, (int)HttpStatusCode.Forbidden);

        var contextMock = new Mock<HttpContext>();
        contextMock.SetupGet(c => c.Response).Returns(responseMock.Object);

        var middleware = new NoelleAuthorizationErrorResponseMiddleware(next, localizerMock.Object);
        await middleware.InvokeAsync(contextMock.Object);

        Assert.Equal((int)HttpStatusCode.Forbidden, responseMock.Object.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_StatusCodeNot403_ShouldNotModifyResponse()
    {
        var next = (RequestDelegate)(_ => Task.CompletedTask);
        var localizerMock = new Mock<IStringLocalizer<NoelleExceptionHandlingResource>>();

        var context = new DefaultHttpContext();
        context.Response.StatusCode = (int)HttpStatusCode.OK;

        var middleware = new NoelleAuthorizationErrorResponseMiddleware(next, localizerMock.Object);
        await middleware.InvokeAsync(context);

        Assert.Equal((int)HttpStatusCode.OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_Forbidden_ShouldWriteErrorResponse()
    {
        var next = (RequestDelegate)(_ => Task.CompletedTask);
        var localizerMock = new Mock<IStringLocalizer<NoelleExceptionHandlingResource>>();
        localizerMock.Setup(l => l["ForbiddenErrorMessage"]).Returns(new LocalizedString("ForbiddenErrorMessage", "Forbidden"));

        var responseBody = new MemoryStream();
        var context = new DefaultHttpContext();
        context.Response.Body = responseBody;
        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;

        var middleware = new NoelleAuthorizationErrorResponseMiddleware(next, localizerMock.Object);
        await middleware.InvokeAsync(context);

        responseBody.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(responseBody);
        var body = await reader.ReadToEndAsync();

        Assert.Contains("Forbidden", body);
        Assert.Contains("error", body.ToLower());
    }
}

public class NoelleAuthorizationExtensionsTests
{
    [Fact]
    public void UseAuthorizationErrorResponse_ShouldCallUseMiddleware()
    {
        var builderMock = new Mock<IApplicationBuilder>();
        builderMock.Setup(b => b.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
            .Returns(builderMock.Object);

        var result = builderMock.Object.UseAuthorizationErrorResponse();

        Assert.NotNull(result);
        builderMock.Verify(b => b.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Once);
    }
}
