namespace NoelleNet.AspNetCore.ExceptionHandling;

/// <summary>
/// 定义错误响应写入器，用于将异常信息写入 HTTP 响应
/// </summary>
public interface IErrorResponseWriter
{
    /// <summary>
    /// 尝试将异常信息写入 HTTP 响应
    /// </summary>
    /// <param name="httpContext">当前请求的 <see cref="HttpContext"/> 实例</param>
    /// <param name="exception">需要处理的 <see cref="Exception"/> 实例</param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns>如果响应已成功写入，则返回 true；否则返回 false</returns>
    ValueTask<bool> TryWriteAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken = default);
}
