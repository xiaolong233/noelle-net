namespace NoelleNet.Http;

/// <summary>
/// 远程调用中的验证错误信息
/// </summary>
public class RemoteCallValidationErrorInfo
{
    /// <summary>
    /// 创建一个新的 <see cref="RemoteCallValidationErrorInfo"/> 实例
    /// </summary>
    /// <param name="message">错误信息</param>
    public RemoteCallValidationErrorInfo(string message)
    {
        Message = message;
    }

    /// <summary>
    /// 创建一个新的 <see cref="RemoteCallValidationErrorInfo"/> 实例
    /// </summary>
    /// <param name="message">错误信息</param>
    /// <param name="members">关联的字段成员</param>
    public RemoteCallValidationErrorInfo(string message, params IEnumerable<string> members) : this(message)
    {
        Members = members;
    }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 关联的字段成员
    /// </summary>
    public IEnumerable<string> Members { get; set; } = [];
}
