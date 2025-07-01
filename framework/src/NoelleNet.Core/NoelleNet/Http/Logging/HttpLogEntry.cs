namespace NoelleNet.Http.Logging;

/// <summary>
/// HttpClient日志信息
/// </summary>
public class HttpLogEntry
{
    /// <summary>
    /// 关联Id
    /// </summary>
    public required string CorrelationId { get; set; }

    /// <summary>
    /// HTTP方法
    /// </summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// 请求URI
    /// </summary>
    public string? RequestUri { get; set; }

    /// <summary>
    /// 请求时间戳
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 请求头
    /// </summary>
    public Dictionary<string, string>? RequestHeaders { get; set; }

    /// <summary>
    /// 请求体报文
    /// </summary>
    public string? RequestBody { get; set; }

    /// <summary>
    /// HTTP状态码
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// 响应头
    /// </summary>
    public Dictionary<string, string>? ResponseHeaders { get; set; }

    /// <summary>
    /// 响应体报文
    /// </summary>
    public string? ResponseBody { get; set; }

    /// <summary>
    /// 时长，单位：毫秒
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// 异常信息
    /// </summary>
    public string? Exception { get; set; }
}