namespace NoelleNet.Http.Logging;

/// <summary>
/// <see cref="HttpClientLoggingOptions"/> 的契约测试：默认脱敏开关与敏感清单是安全契约，防止默认值被无意放宽
/// </summary>
public class HttpClientLoggingOptionsTests
{
    /// <summary>
    /// 默认值：脱敏开启、请求/响应体长度上限、清单非空
    /// </summary>
    [Fact]
    public void Defaults_ShouldEnableSanitization()
    {
        var options = new HttpClientLoggingOptions();

        Assert.True(options.SanitizeSensitiveData);
        Assert.Equal(8192, options.MaxRequestBodyLength);
        Assert.Equal(16384, options.MaxResponseBodyLength);
        Assert.NotEmpty(options.SensitiveFields);
        Assert.NotEmpty(options.SensitiveHeaders);
    }

    /// <summary>
    /// 敏感字段与敏感标头的默认清单应包含关键凭证项（Authorization/Cookie 等）
    /// </summary>
    [Fact]
    public void SensitiveLists_ShouldContainCredentialKeys()
    {
        var options = new HttpClientLoggingOptions();

        Assert.Contains("password", options.SensitiveFields);
        Assert.Contains("token", options.SensitiveFields);
        Assert.Contains("creditcard", options.SensitiveFields);
        Assert.Contains("cvv", options.SensitiveFields);
        Assert.Contains("authorization", options.SensitiveFields);

        Assert.Contains("Authorization", options.SensitiveHeaders);
        Assert.Contains("Cookie", options.SensitiveHeaders);
        Assert.Contains("Set-Cookie", options.SensitiveHeaders);
        Assert.Contains("X-Api-Key", options.SensitiveHeaders);
    }
}
