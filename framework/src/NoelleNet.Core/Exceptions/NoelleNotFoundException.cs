namespace NoelleNet.Core.Exceptions;

/// <summary>
/// 当目标资源未找到时引发的异常
/// </summary>
/// <param name="message">错误消息字符串</param>
/// <param name="innerException">内部异常引用</param>
public class NoelleNotFoundException(string? message = default, Exception? innerException = default) : Exception(message ?? "未找到目标资源", innerException)
{
}
