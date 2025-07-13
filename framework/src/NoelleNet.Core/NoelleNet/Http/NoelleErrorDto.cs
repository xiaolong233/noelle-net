namespace NoelleNet.Http;

/// <summary>
/// 错误详细信息的数据传输对象
/// </summary>
/// <param name="message">错误消息</param>
public class NoelleErrorDto
{
    public NoelleErrorDto(string code, string message)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    /// <summary>
    /// 错误代码
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 错误的具体目标（如参数名或字段名）
    /// </summary>
    public string? Target { get; set; }

    /// <summary>
    /// 错误详情
    /// </summary>
    public IEnumerable<NoelleErrorDto>? Details { get; set; }

    /// <summary>
    /// 内部错误
    /// </summary>
    public NoelleInnerErrorDto? InnerError { get; set; }
}
