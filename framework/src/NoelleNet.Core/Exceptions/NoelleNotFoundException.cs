namespace NoelleNet.Core.Exceptions;

/// <summary>
/// 当目标资源未找到时引发的异常
/// </summary>
public class NoelleNotFoundException : Exception
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="message">错误消息字符串</param>
    /// <param name="innerException">内部异常引用</param>
    public NoelleNotFoundException(string message, Exception? innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="message">错误消息字符串</param>
    public NoelleNotFoundException(string message) : base(message)
    { 
    }
}
