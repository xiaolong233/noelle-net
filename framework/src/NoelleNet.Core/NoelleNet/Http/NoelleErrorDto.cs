namespace NoelleNet.Http;

/// <summary>
/// 错误详细信息的数据传输对象
/// </summary>
/// <param name="message">错误消息</param>
public class NoelleErrorDto
{
    /// <summary>
    /// 创建一个新的 <see cref="NoelleErrorDto"/> 实例
    /// </summary>
    /// <param name="message">错误信息</param>
    /// <exception cref="ArgumentNullException"></exception>
    public NoelleErrorDto(string message)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    /// <summary>
    /// 创建一个新的 <see cref="NoelleErrorDto"/> 实例
    /// </summary>
    /// <param name="code">错误代码</param>
    /// <param name="message">错误信息</param>
    /// <exception cref="ArgumentNullException"></exception>
    public NoelleErrorDto(string code, string message)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    /// <summary>
    /// 错误代码
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// 错误信息
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
    public dynamic? InnerError { get; set; }
}
