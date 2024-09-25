using NoelleNet.Core.Exceptions;

namespace NoelleNet.Http;

/// <summary>
/// 远程调用异常类，继承自 <see cref="Exception"/>，并实现 <see cref="IHasErrorCode"/> 和 <see cref="IHasHttpStatusCode"/> 接口
/// </summary>
public class NoelleRemoteCallException : Exception, IHasErrorCode, IHasHttpStatusCode
{
    /// <summary>
    /// 获取或设置错误代码
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// 获取或设置HTTP状态码
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// 获取或设置发生远程调用的URL地址
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// 获取或设置发生远程调用的HTTP方法
    /// </summary>
    public string? Method { get; set; }

    /// <summary>
    /// 获取或设置发送的请求数据
    /// </summary>
    public object? Payload { get; set; }

    /// <summary>
    /// 初始化 <see cref="NoelleRemoteCallException"/> 类的新实例
    /// </summary>
    public NoelleRemoteCallException()
    {
    }

    /// <summary>
    /// 使用指定的错误信息和内部异常初始化 <see cref="NoelleRemoteCallException"/> 类的新实例
    /// </summary>
    /// <param name="message">错误信息</param>
    /// <param name="innerException">内部异常对象</param>
    public NoelleRemoteCallException(string message, Exception? innerException) : base(message, innerException)
    {
    }
}
