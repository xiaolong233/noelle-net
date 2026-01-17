using NoelleNet.ExceptionHandling;

namespace NoelleNet;

/// <summary>
/// 业务异常类，继承自 <see cref="Exception"/>，并实现 <see cref="IHasErrorCode"/> 接口。
/// </summary>
public class BusinessException : Exception, IBusinessException, IHasErrorCode
{
    /// <summary>
    /// 创建一个新的 <see cref="BusinessException"/> 实例
    /// </summary>
    public BusinessException()
    {
    }

    /// <summary>
    /// 创建一个新的 <see cref="BusinessException"/> 实例
    /// </summary>
    /// <param name="message">错误信息</param>
    public BusinessException(string message) : base(message)
    {
    }

    /// <summary>
    /// 创建一个新的 <see cref="BusinessException"/> 实例
    /// </summary>
    /// <param name="message">错误信息</param>
    /// <param name="innerException">内部异常实例</param>
    public BusinessException(string message, Exception? innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// 创建一个新的 <see cref="BusinessException"/> 实例
    /// </summary>
    /// <param name="errorCode">错误代码</param>
    /// <param name="message">错误信息</param>
    public BusinessException(string errorCode, string? message) : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// 创建一个新的 <see cref="BusinessException"/> 实例
    /// </summary>
    /// <param name="errorCode">错误代码</param>
    /// <param name="message">错误信息</param>
    /// <param name="innerException">内部异常实例</param>
    public BusinessException(string errorCode, string? message, Exception? innerException) : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    /// <inheritdoc/>
    public string? ErrorCode { get; set; }
}
