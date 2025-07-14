using NoelleNet.ExceptionHandling;

namespace NoelleNet.Http;

/// <summary>
/// 远程调用异常类，继承自 <see cref="Exception"/>，并实现 <see cref="IHasErrorCode"/> 和 <see cref="IHasHttpStatusCode"/> 接口
/// </summary>
public class NoelleRemoteCallException : Exception, IHasErrorCode, IHasHttpStatusCode
{
    /// <summary>
    /// 错误代码
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// HTTP状态码
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// 发生远程调用的URL地址
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// 发生远程调用的HTTP方法
    /// </summary>
    public string? Method { get; set; }

    /// <summary>
    /// 发送的请求数据
    /// </summary>
    public object? Payload { get; set; }

    /// <summary>
    /// 错误详情 <see cref="NoelleErrorDto"/> 
    /// </summary>
    public NoelleErrorDto? ErrorDetail { get; set; }

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
    public NoelleRemoteCallException(string message, Exception? innerException) : base(message, innerException)
    {
    }
}
