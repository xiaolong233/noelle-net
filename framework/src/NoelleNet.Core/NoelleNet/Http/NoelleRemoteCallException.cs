using NoelleNet.ExceptionHandling;

namespace NoelleNet.Http;

/// <summary>
/// 远程调用异常类
/// </summary>
public class NoelleRemoteCallException : Exception, IHasErrorCode, IHasHttpStatusCode, IHasErrorDetails
{
    /// <summary>
    /// 创建一个新的 <see cref="NoelleRemoteCallException"/> 实例
    /// </summary>
    public NoelleRemoteCallException()
    {
    }

    /// <summary>
    /// 创建一个新的 <see cref="NoelleRemoteCallException"/> 实例
    /// </summary>
    /// <param name="message">错误信息</param>
    /// <param name="innerException">内部异常对象</param>
    public NoelleRemoteCallException(string message, Exception? innerException = null) : base(message, innerException)
    {
    }

    /// <summary>
    /// 创建一个新的 <see cref="NoelleRemoteCallException"/> 实例
    /// </summary>
    /// <param name="error">远程调用时的错误信息</param>
    /// <param name="innerException">内部异常对象</param>
    public NoelleRemoteCallException(RemoteCallErrorInfo error, Exception? innerException = null) : base(error.Message, innerException)
    {
        Error = error;

        if (error != null && error.Data != null)
        {
            foreach (var key in error.Data.Keys)
            {
                this.Data[key] = error.Data[key];
            }
        }
    }

    /// <inheritdoc/>
    public string? ErrorCode { get; set; }

    /// <inheritdoc/>
    public int StatusCode { get; set; }

    /// <inheritdoc/>
    public string? Details { get; set; }

    /// <summary>
    /// 远程调用时的错误信息
    /// </summary>
    public RemoteCallErrorInfo? Error { get; set; }
}
