namespace NoelleNet.Http.Logging;

public class HttpClientLoggingOptionsTests
{
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        var options = new HttpClientLoggingOptions();

        Assert.Equal(8192, options.MaxRequestBodyLength);
        Assert.Equal(16384, options.MaxResponseBodyLength);
        Assert.True(options.SanitizeSensitiveData);
        Assert.NotNull(options.SensitiveFields);
        Assert.NotNull(options.SensitiveHeaders);
    }

    [Fact]
    public void SensitiveFields_ShouldContainExpectedDefaults()
    {
        var options = new HttpClientLoggingOptions();

        Assert.Contains("password", options.SensitiveFields);
        Assert.Contains("token", options.SensitiveFields);
        Assert.Contains("creditcard", options.SensitiveFields);
        Assert.Contains("cvv", options.SensitiveFields);
        Assert.Contains("authorization", options.SensitiveFields);
    }

    [Fact]
    public void SensitiveHeaders_ShouldContainExpectedDefaults()
    {
        var options = new HttpClientLoggingOptions();

        Assert.Contains("Authorization", options.SensitiveHeaders);
        Assert.Contains("Cookie", options.SensitiveHeaders);
        Assert.Contains("Set-Cookie", options.SensitiveHeaders);
        Assert.Contains("X-Api-Key", options.SensitiveHeaders);
    }

    [Fact]
    public void HttpClientLogging_Constant_ShouldBeCorrect()
    {
        Assert.Equal("HttpClientLogging", HttpClientLoggingOptions.HttpClientLogging);
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        var options = new HttpClientLoggingOptions
        {
            MaxRequestBodyLength = 1024,
            MaxResponseBodyLength = 2048,
            SanitizeSensitiveData = false,
            SensitiveFields = new List<string> { "custom" },
            SensitiveHeaders = new List<string> { "X-Custom" }
        };

        Assert.Equal(1024, options.MaxRequestBodyLength);
        Assert.Equal(2048, options.MaxResponseBodyLength);
        Assert.False(options.SanitizeSensitiveData);
        Assert.Single(options.SensitiveFields);
        Assert.Single(options.SensitiveHeaders);
    }
}
