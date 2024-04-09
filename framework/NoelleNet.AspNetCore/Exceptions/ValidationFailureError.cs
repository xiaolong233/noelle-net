namespace NoelleNet.AspNetCore.Exceptions;

/// <summary>
/// 验证失败的错误信息
/// </summary>
public class ValidationFailureError : Error
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="message">错误信息</param>
    /// <param name="memberName">验证失败的成员名称</param>
    public ValidationFailureError(string message, string memberName) : base(message)
    {
        MemberName = memberName;
    }

    /// <summary>
    /// 验证失败的成员名称
    /// </summary>
    public string MemberName { get; }
}
