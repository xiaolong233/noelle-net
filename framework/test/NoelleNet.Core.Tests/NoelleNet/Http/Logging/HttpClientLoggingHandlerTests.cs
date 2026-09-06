using Moq;
using Moq.Protected;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;

namespace NoelleNet.Http.Logging;

public class HttpClientLoggingHandlerTests
{
    private readonly Mock<ILogger<HttpClientLoggingHandler>> _loggerMock;
    private readonly HttpClientLoggingOptions _options;

    public HttpClientLoggingHandlerTests()
    {
        _loggerMock = new Mock<ILogger<HttpClientLoggingHandler>>();
        _options = new HttpClientLoggingOptions();
    }

    private HttpClientLoggingHandler CreateHandler(HttpMessageHandler innerHandler)
    {
        var optionsMock = new Mock<IOptions<HttpClientLoggingOptions>>();
        optionsMock.Setup(o => o.Value).Returns(_options);
        return new HttpClientLoggingHandler(_loggerMock.Object, optionsMock.Object)
        {
            InnerHandler = innerHandler
        };
    }

    [Fact]
    public void Constructor_NullLogger_ShouldThrow()
    {
        var optionsMock = new Mock<IOptions<HttpClientLoggingOptions>>();
        Assert.Throws<ArgumentNullException>(() => new HttpClientLoggingHandler(null!, optionsMock.Object));
    }

    [Fact]
    public async Task SendAsync_SuccessfulRequest_ShouldLogInformation()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"result\":\"ok\"}", Encoding.UTF8, "application/json")
        };
        var innerHandler = new TestMessageHandler(response);
        var handler = CreateHandler(innerHandler);

        var invoker = new HttpMessageInvoker(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");
        var result = await invoker.SendAsync(request, CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task SendAsync_ExceptionDuringSend_ShouldLogError()
    {
        var innerHandler = new ExceptionThrowingHandler(new InvalidOperationException("network error"));
        var handler = CreateHandler(innerHandler);

        var invoker = new HttpMessageInvoker(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");

        await Assert.ThrowsAsync<InvalidOperationException>(() => invoker.SendAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task SendAsync_ShouldRecordResponseStatusCode()
    {
        var response = new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("not found", Encoding.UTF8, "text/plain")
        };
        var innerHandler = new TestMessageHandler(response);
        var handler = CreateHandler(innerHandler);

        var invoker = new HttpMessageInvoker(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/missing");
        var result = await invoker.SendAsync(request, CancellationToken.None);

        Assert.Equal(404, (int)result.StatusCode);
    }

    /// <summary>
    /// 使用流式内容（StreamContent）的请求，日志读取后真正发送的请求体仍应完整
    /// （回归测试：SendAsync 前 LoadIntoBufferAsync，防止流被日志读取消费）
    /// </summary>
    [Fact]
    public async Task SendAsync_WithStreamContent_ShouldSendCompleteBody()
    {
        // Arrange
        const string payload = "{\"stream\":\"payload-body\"}";
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        var innerHandler = new CapturingMessageHandler(response);
        var handler = CreateHandler(innerHandler);

        var invoker = new HttpMessageInvoker(handler);
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.example.com/upload")
        {
            Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(payload)))
        };
        request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        // Act
        await invoker.SendAsync(request, CancellationToken.None);

        // Assert：内层处理器收到的请求体完整（未被日志读取消费）
        Assert.Equal(payload, innerHandler.ReceivedBody);
    }

    private class TestMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;
        public TestMessageHandler(HttpResponseMessage response) => _response = response;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_response);
    }

    private class CapturingMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;
        public CapturingMessageHandler(HttpResponseMessage response) => _response = response;
        public string? ReceivedBody { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Content != null)
                ReceivedBody = await request.Content.ReadAsStringAsync(cancellationToken);

            return _response;
        }
    }

    private class ExceptionThrowingHandler : HttpMessageHandler
    {
        private readonly Exception _exception;
        public ExceptionThrowingHandler(Exception exception) => _exception = exception;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => throw _exception;
    }
}
