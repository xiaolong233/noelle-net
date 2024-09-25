namespace NoelleNet.AspNetCore.ExceptionHandling.Models;

/// <summary>
/// 错误详细信息
/// </summary>
/// <param name="message">错误消息</param>
public class NoelleErrorDetailDto(string message)
{
    /// <summary>
    /// 错误代码
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string Message { get; set; } = message;

    /// <summary>
    /// 错误的具体目标（如参数名或字段名）
    /// </summary>
    public string? Target { get; set; }

    /// <summary>
    /// 错误详情
    /// </summary>
    public IEnumerable<NoelleErrorDetailDto>? Details { get; set; }

    /// <summary>
    /// 额外信息，通常用于调试，包含更详细的技术信息
    /// </summary>
    public dynamic? Extra { get; set; }
}
