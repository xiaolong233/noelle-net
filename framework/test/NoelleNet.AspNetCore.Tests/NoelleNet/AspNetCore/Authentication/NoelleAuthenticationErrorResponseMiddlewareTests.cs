using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Moq;
using NoelleNet.AspNetCore.ExceptionHandling.Localization;
using System.Net;

namespace NoelleNet.AspNetCore.Authentication;

public class NoelleAuthenticationErrorResponseMiddlewareTests
{
    [Fact]
    public void Constructor_NullNext_ShouldThrowArgumentNullException()
    {
        var localizerMock = new Mock<IStringLocalizer<NoelleExceptionHandlingResource>>();

        Assert.Throws<ArgumentNullException>(() =>
            new NoelleAuthenticationErrorResponseMiddleware(null!, localizerMock.Object));
    }

    [Fact]
    public void Constructor_NullLocalizer_ShouldThrowArgumentNullException()
    {
        var next = (RequestDelegate)(_ => Task.CompletedTask);

        Assert.Throws<ArgumentNullException>(() =>
            new NoelleAuthenticationErrorResponseMiddleware(next, null!));
    }

    [Fact]
    public async Task InvokeAsync_ResponseHasStarted_ShouldNotModifyResponse()
    {
        var next = (RequestDelegate)(_ => Task.CompletedTask);
        var localizerMock = new Mock<IStringLocalizer<NoelleExceptionHandlingResource>>();

        var responseMock = new Mock<HttpResponse>();
        responseMock.SetupGet(r => r.HasStarted).Returns(true);
        responseMock.SetupProperty(r => r.StatusCode, (int)HttpStatusCode.Unauthorized);

        var contextMock = new Mock<HttpContext>();
        contextMock.SetupGet(c => c.Response).Returns(responseMock.Object);

        var middleware = new NoelleAuthenticationErrorResponseMiddleware(next, localizerMock.Object);
        await middleware.InvokeAsync(contextMock.Object);

        // Assert response was not modified (WriteAsJsonAsync won't be called since HasStarted is true)
        Assert.Equal((int)HttpStatusCode.Unauthorized, responseMock.Object.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_StatusCodeNot401_ShouldNotModifyResponse()
    {
        var next = (RequestDelegate)(_ => Task.CompletedTask);
        var localizerMock = new Mock<IStringLocalizer<NoelleExceptionHandlingResource>>();

        var context = new DefaultHttpContext();
        context.Response.StatusCode = (int)HttpStatusCode.OK;

        var middleware = new NoelleAuthenticationErrorResponseMiddleware(next, localizerMock.Object);
        await middleware.InvokeAsync(context);

        Assert.Equal((int)HttpStatusCode.OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_Unauthorized_ShouldWriteErrorResponse()
    {
        var next = (RequestDelegate)(_ => Task.CompletedTask);
        var localizerMock = new Mock<IStringLocalizer<NoelleExceptionHandlingResource>>();
        localizerMock.Setup(l => l["UnauthorizedErrorMessage"]).Returns(new LocalizedString("UnauthorizedErrorMessage", "Unauthorized"));

        var responseBody = new MemoryStream();
        var context = new DefaultHttpContext();
        context.Response.Body = responseBody;
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

        var middleware = new NoelleAuthenticationErrorResponseMiddleware(next, localizerMock.Object);
        await middleware.InvokeAsync(context);

        // Read the response body to verify
        responseBody.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(responseBody);
        var body = await reader.ReadToEndAsync();

        Assert.Contains("Unauthorized", body);
        Assert.Contains("error", body.ToLower());
    }
}

public class NoelleAuthenticationExtensionsTests
{
    [Fact]
    public void UseAuthenticationErrorResponse_ShouldCallUseMiddleware()
    {
        var builderMock = new Mock<IApplicationBuilder>();
        builderMock.Setup(b => b.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
            .Returns(builderMock.Object);

        var result = builderMock.Object.UseAuthenticationErrorResponse();

        Assert.NotNull(result);
        builderMock.Verify(b => b.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Once);
    }
}
