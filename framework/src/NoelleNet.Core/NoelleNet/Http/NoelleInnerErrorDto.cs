namespace NoelleNet.Http;

/// <summary>
/// 内部错误信息的数据传输对象
/// </summary>
public class NoelleInnerErrorDto
{
    /// <summary>
    /// 创建一个新的 <see cref="NoelleInnerErrorDto"/> 实例
    /// </summary>
    public NoelleInnerErrorDto()
    {
    }

    /// <summary>
    /// 创建一个新的 <see cref="NoelleInnerErrorDto"/> 实例
    /// </summary>
    /// <param name="code">错误代码</param>
    /// <param name="innerError">内部错误信息</param>
    public NoelleInnerErrorDto(string? code, NoelleInnerErrorDto? innerError = null)
    {
        Code = code;
        InnerError = innerError;
    }

    /// <summary>
    /// 错误代码
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// 内部错误信息
    /// </summary>
    public NoelleInnerErrorDto? InnerError { get; set; }
}
