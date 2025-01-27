using System.Net;

namespace NoelleNet.AspNetCore.ExceptionHandling;

/// <summary>
/// Http异常状态码查找器
/// </summary>
public interface IHttpExceptionStatusCodeFinder
{
    /// <summary>
    /// 根据 <see cref="Exception"/> 获取HTTP状态码
    /// </summary>
    /// <param name="context">HTTP请求上下文</param>
    /// <param name="exception">异常信息</param>
    /// <returns></returns>
    HttpStatusCode GetStatusCode(HttpContext context, Exception exception);
}