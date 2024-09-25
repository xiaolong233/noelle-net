namespace NoelleNet.Core.Exceptions;

/// <summary>
/// 表示状态冲突的 <see cref="Exception"/> 类
/// </summary>
public class NoelleConflictException : Exception
{
    /// <summary>
    /// 初始化 <see cref="NoelleConflictException"/> 类的新实例
    /// </summary>
    public NoelleConflictException()
    {
    }

    /// <summary>
    /// 使用指定的错误信息和内部异常初始化 <see cref="NoelleConflictException"/> 类的新实例
    /// </summary>
    /// <param name="message">错误信息</param>
    /// <param name="innerException">内部异常对象</param>
    public NoelleConflictException(string message, Exception? innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// 使用指定的错误信息初始化 <see cref="NoelleConflictException"/> 类的新实例
    /// </summary>
    /// <param name="message">错误信息</param>
    public NoelleConflictException(string message) : base(message)
    {
    }
}
