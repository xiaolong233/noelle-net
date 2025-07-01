namespace NoelleNet.Http.Logging;

/// <summary>
/// HttpClient日志选项
/// </summary>
public class HttpClientLoggingOptions
{
    /// <summary>
    /// 配置节点名称
    /// </summary>
    public const string HttpClientLogging = "HttpClientLogging";

    /// <summary>
    /// 记录请求体的最大长度，默认值：8192
    /// </summary>
    public int MaxRequestBodyLength { get; set; } = 8192;  // 8KB

    /// <summary>
    /// 记录响应体的最大长度，默认值：16384
    /// </summary>
    public int MaxResponseBodyLength { get; set; } = 16384; // 16KB

    /// <summary>
    /// 是否清理敏感数据
    /// </summary>
    public bool SanitizeSensitiveData { get; set; } = true;

    /// <summary>
    /// 敏感字段
    /// </summary>
    public List<string> SensitiveFields { get; set; } =
    [
        "password",
        "token",
        "creditcard",
        "cvv",
        "authorization"
    ];

    /// <summary>
    /// 敏感标头
    /// </summary>
    public List<string> SensitiveHeaders { get; set; } =
    [
        "Authorization",
        "Cookie",
        "Set-Cookie",
        "X-Api-Key"
    ];
}
