using System.Collections;

namespace NoelleNet.Http;

/// <summary>
/// 远程调用时的错误信息
/// </summary>
public class RemoteCallErrorInfo
{
    /// <summary>
    /// 创建一个新的 <see cref="RemoteCallErrorInfo"/> 实例
    /// </summary>
    public RemoteCallErrorInfo()
    {
    }

    /// <summary>
    /// 创建一个新的 <see cref="RemoteCallErrorInfo"/> 实例
    /// </summary>
    /// <param name="message">错误信息</param>
    /// <param name="code">错误代码</param>
    /// <param name="details">错误详情</param>
    /// <param name="data">附加数据</param>
    public RemoteCallErrorInfo(string message, string? code = null, string? details = null, IDictionary? data = null)
    {
        Message = message;
        Code = code;
        Details = details;
        Data = data;
    }

    /// <summary>
    /// 错误代码
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 错误详情
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// 附加数据
    /// </summary>
    public IDictionary? Data { get; set; }

    /// <summary>
    /// 模型验证错误信息
    /// </summary>
    public IEnumerable<RemoteCallValidationErrorInfo>? ValidationErrors { get; set; }
}
