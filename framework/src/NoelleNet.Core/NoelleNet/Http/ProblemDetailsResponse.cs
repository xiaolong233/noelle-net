using System.Text.Json.Serialization;

namespace NoelleNet.Http;

/// <summary>
/// 一种用于指定 HTTP API 响应中错误的机器可读格式，基于 <see href="https://tools.ietf.org/html/rfc7807"/>
/// </summary>
public class ProblemDetailsResponse
{
    /// <summary>
    /// 一个 URI 引用 [RFC3986]，用于标识问题类型。建议在取消引用时提供人类可读的文档（例如，使用 HTML [W3C.REC-html5-20141028]）。当此成员不存在时，其值假定为 "about:blank"
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyOrder(-5)]
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// 问题类型的简短、人类可读的摘要。除非出于本地化目的（例如，使用主动内容协商；参见 [RFC7231] 第 3.4 节），否则它不应在问题的每次发生中发生变化
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyOrder(-4)]
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// 由源服务器为该问题的每次发生生成的 HTTP 状态码（[RFC7231] 第 6 节）
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyOrder(-3)]
    [JsonPropertyName("status")]
    public int? Status { get; set; }

    /// <summary>
    /// 针对该具体问题实例的、易于理解的解释
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyOrder(-2)]
    [JsonPropertyName("detail")]
    public string? Detail { get; set; }

    /// <summary>
    /// 一个 URI 引用，用于标识问题的具体实例。如果取消引用，它可能会产生进一步的信息，也可能不会
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyOrder(-1)]
    [JsonPropertyName("instance")]
    public string? Instance { get; set; }

    /// <summary>
    /// 获取扩展成员的 <see cref="IDictionary{TKey, TValue}"/>
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, object?> Extensions { get; set; } = new Dictionary<string, object?>(StringComparer.Ordinal);

    /// <summary>
    /// 模型验证错误信息。键为字段名，值为该字段的错误信息数组
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("errors")]
    public IDictionary<string, string[]>? Errors { get; set; }
}
