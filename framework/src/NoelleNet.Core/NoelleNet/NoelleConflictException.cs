namespace NoelleNet;

/// <summary>
/// 表示状态冲突的 <see cref="Exception"/> 类
/// </summary>
public class NoelleConflictException : Exception
{
    /// <summary>
    /// 创建一个新的 <see cref="NoelleConflictException"/> 实例
    /// </summary>
    public NoelleConflictException()
    {
    }

    /// <summary>
    /// 创建一个新的 <see cref="NoelleConflictException"/> 新实例
    /// </summary>
    /// <param name="message">错误信息</param>
    /// <param name="innerException">内部异常对象</param>
    public NoelleConflictException(string message, Exception? innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// 创建一个新的 <see cref="NoelleConflictException"/> 实例
    /// </summary>
    /// <param name="message">错误信息</param>
    public NoelleConflictException(string message) : base(message)
    {
    }
}
