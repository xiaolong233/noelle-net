using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace NoelleNet.Http.Logging;

/// <summary>
/// HttpClient日志记录器
/// </summary>
public class HttpClientLoggingHandler : DelegatingHandler
{
    private readonly ILogger<HttpClientLoggingHandler> _logger;
    private readonly HttpClientLoggingOptions _options;

    public HttpClientLoggingHandler(ILogger<HttpClientLoggingHandler> logger, IOptions<HttpClientLoggingOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options.Value;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var logEntry = new HttpLogEntry
        {
            CorrelationId = Guid.NewGuid().ToString(),
            Method = request.Method.Method,
            RequestUri = request.RequestUri?.ToString(),
            Timestamp = DateTime.UtcNow
        };

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // 记录请求头
            logEntry.RequestHeaders = GetSafeHeaders(request.Headers);

            // 处理请求体
            if (request.Content != null)
            {
                var mediaType = request.Content.Headers.ContentType?.MediaType ?? string.Empty;
                var body = await request.Content.ReadAsStringAsync(cancellationToken) ?? string.Empty;
                logEntry.RequestBody = ProcessContent(body, mediaType, true);
            }

            // 发送请求
            var response = await base.SendAsync(request, cancellationToken);
            stopwatch.Stop();

            // 记录响应信息
            logEntry.StatusCode = (int)response.StatusCode;
            logEntry.ResponseHeaders = GetSafeHeaders(response.Headers);
            logEntry.DurationMs = stopwatch.ElapsedMilliseconds;

            // 记录响应体
            if (response.Content != null)
            {
                var mediaType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
                var body = await response.Content.ReadAsStringAsync(cancellationToken) ?? string.Empty;
                logEntry.ResponseBody = ProcessContent(body, mediaType, false);
            }

            _logger.LogInformation("Outgoing HTTP {HttpMethod} to {HttpUrl} completed with {HttpStatusCode} in {LatencyMs}ms - {@logData}",
                logEntry.Method, logEntry.RequestUri, logEntry.StatusCode, logEntry.DurationMs, logEntry);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logEntry.DurationMs = stopwatch.ElapsedMilliseconds;
            logEntry.Exception = ex.ToString();

            _logger.LogError(ex, "Outgoing HTTP {HttpMethod} to {HttpUrl} failed with {ErrorCode} after {LatencyMs}ms - {@logData}",
                logEntry.Method, logEntry.RequestUri, logEntry.StatusCode, logEntry.DurationMs, logEntry);

            throw;
        }
    }

    /// <summary>
    /// 处理请求/响应的内容
    /// </summary>
    /// <param name="content">待处理的内容</param>
    /// <param name="mediaType">媒体类型</param>
    /// <param name="isRequest">是否为请求</param>
    /// <returns></returns>
    private string ProcessContent(string content, string mediaType, bool isRequest)
    {
        if (string.IsNullOrWhiteSpace(content))
            return "[Empty Content]";

        // 处理二进制内容
        if (IsBinaryContent(mediaType))
        {
            return $"[Binary Data - {mediaType}, Size: {content.Length} bytes]";
        }

        // 敏感信息过滤
        content = SanitizeSensitiveData(content, mediaType);

        // 智能截断（保留关键结构）
        return SmartTruncate(content, isRequest ? _options.MaxRequestBodyLength : _options.MaxResponseBodyLength);
    }

    /// <summary>
    /// 判断是否为二进制内容
    /// </summary>
    /// <param name="mediaType"></param>
    /// <returns></returns>
    private static bool IsBinaryContent(string mediaType)
    {
        return mediaType switch
        {
            string mt when mt.Contains("image/") => true,
            string mt when mt.Contains("audio/") => true,
            string mt when mt.Contains("video/") => true,
            string mt when mt.Contains("application/octet-stream") => true,
            string mt when mt.Contains("application/pdf") => true,
            _ => false
        };
    }

    /// <summary>
    /// 根据最大长度截取内容
    /// </summary>
    /// <param name="content">原内容</param>
    /// <param name="maxLength">最大长度</param>
    /// <returns></returns>
    private static string SmartTruncate(string content, int maxLength)
    {
        if (content.Length <= maxLength) return content;

        // 首尾各保留40%
        int firstPart = (int)(maxLength * 0.4);
        int lastPart = maxLength - firstPart - 10;

        return $"{content[..firstPart]}\n\n" +
               $"-- TRUNCATED {content.Length - maxLength} CHARACTERS --\n\n" +
               $"{content.Substring(content.Length - lastPart, lastPart)}";
    }

    /// <summary>
    /// 清理敏感数据
    /// </summary>
    /// <param name="content">待清理的内容</param>
    /// <param name="mediaType">媒体类型</param>
    /// <returns></returns>
    private string SanitizeSensitiveData(string content, string mediaType)
    {
        if (!_options.SanitizeSensitiveData)
            return content;
        if (string.IsNullOrWhiteSpace(content))
            return content;

        try
        {
            // JSON 敏感字段处理
            if (mediaType.Contains("json"))
            {
                return SanitizeJson(content);
            }

            // FormData 敏感字段处理
            if (mediaType.Contains("x-www-form-urlencoded"))
            {
                return SanitizeFormData(content);
            }
        }
        catch
        {
            // 安全失败：处理失败时返回原始内容            
        }

        return content;
    }

    /// <summary>
    /// 返回脱敏后的Json字符串
    /// </summary>
    /// <param name="json">原始Json字符串</param>
    /// <returns></returns>
    private string SanitizeJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        writer.WriteStartObject();

        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            writer.WritePropertyName(prop.Name);

            if (_options.SensitiveFields.Contains(prop.Name, StringComparer.OrdinalIgnoreCase))
            {
                writer.WriteStringValue("*****");
            }
            else
            {
                prop.Value.WriteTo(writer);
            }
        }

        writer.WriteEndObject();
        writer.Flush();

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// 返回脱敏后的表单数据
    /// </summary>
    /// <param name="formData">原始表单数据</param>
    /// <returns></returns>
    private string SanitizeFormData(string formData)
    {
        var pairs = formData.Split('&')
            .Select(x =>
            {
                var keyValue = x.Split('=');
                if (keyValue.Length != 2) return x;

                var key = Uri.UnescapeDataString(keyValue[0]);
                var value = Uri.UnescapeDataString(keyValue[1]);

                if (_options.SensitiveFields.Contains(key, StringComparer.OrdinalIgnoreCase))
                {
                    return $"{key}=*****";
                }

                return $"{key}={value}";
            });

        return string.Join("&", pairs);
    }

    /// <summary>
    /// 返回脱敏后的标头
    /// </summary>
    /// <param name="headers">原始标头</param>
    /// <returns></returns>
    private Dictionary<string, string> GetSafeHeaders(HttpHeaders headers)
    {
        return headers
            .Where(x => !_options.SensitiveHeaders.Contains(x.Key, StringComparer.OrdinalIgnoreCase))
            .ToDictionary(x => x.Key, x => string.Join(";", x.Value));
    }
}
