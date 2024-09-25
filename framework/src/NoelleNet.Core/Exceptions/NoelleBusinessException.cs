namespace NoelleNet.Core.Exceptions;

/// <summary>
/// 业务异常类，继承自 <see cref="Exception"/>，并实现 <see cref="IHasErrorCode"/> 接口。
/// </summary>
public class NoelleBusinessException : Exception, IHasErrorCode
{
    /// <summary>
    /// 获取或设置错误代码
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// 使用指定的错误代码、错误信息和内部异常初始化 <see cref="NoelleBusinessException"/> 类的新实例
    /// </summary>
    /// <param name="errorCode">错误代码</param>
    /// <param name="message">异常消息</param>
    /// <param name="innerException">内部异常对象</param>
    public NoelleBusinessException(string errorCode, string message, Exception? innerException) : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// 使用指定的错误信息和内部异常初始化 <see cref="NoelleBusinessException"/> 类的新实例
    /// </summary>
    /// <param name="message">错误信息</param>
    /// <param name="innerException">内部异常对象</param>
    public NoelleBusinessException(string message, Exception? innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// 使用指定的错误信息初始化 <see cref="NoelleBusinessException"/> 类的新实例
    /// </summary>
    /// <param name="message">错误信息</param>
    public NoelleBusinessException(string message) : base(message)
    {
    }
}
