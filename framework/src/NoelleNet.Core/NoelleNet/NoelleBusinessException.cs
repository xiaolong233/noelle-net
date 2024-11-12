using NoelleNet.ExceptionHandling;

namespace NoelleNet;

/// <summary>
/// 业务异常类，继承自 <see cref="Exception"/>，并实现 <see cref="IHasErrorCode"/> 接口。
/// </summary>
/// <param name="errorCode">错误代码</param>
/// <param name="message">异常消息</param>
/// <param name="innerException">内部异常对象</param>
public class NoelleBusinessException(string? errorCode = default, string? message = default, Exception? innerException = default) : Exception(message, innerException), IHasErrorCode
{
    /// <summary>
    /// 获取或设置错误代码
    /// </summary>
    public string? ErrorCode { get; set; } = errorCode;
}
