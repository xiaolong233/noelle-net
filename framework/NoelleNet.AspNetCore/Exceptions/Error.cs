using System.Collections;

namespace NoelleNet.AspNetCore.Exceptions;

/// <summary>
/// 通用的错误信息
/// </summary>
public class Error
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="message">错误消息</param>
    public Error(string message)
    {
        Message = message;
    }

    /// <summary>
    /// 错误代码
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 错误详情
    /// </summary>
    public IEnumerable<Error>? Details { get; set; }

    /// <summary>
    /// 额外数据
    /// </summary>
    public IDictionary? Extra { get; set; }
}
