namespace NoelleNet.AspNetCore.Exceptions;

/// <summary>
/// 可以实现此接口以将 <see cref="Exception"/> 对象转换为 <see cref="Error"/> 对象。
/// </summary>
public interface IExceptionToErrorConverter
{
    /// <summary>
    /// 转换处理
    /// </summary>
    /// <param name="exception">需要进行转换处理的 <see cref="Exception"/> 对象</param>
    /// <returns></returns>
    Error Covert(Exception exception);
}
