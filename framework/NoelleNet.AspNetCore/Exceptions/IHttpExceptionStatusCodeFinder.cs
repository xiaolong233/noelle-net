using System.Net;

namespace NoelleNet.AspNetCore.Exceptions;

/// <summary>
/// 根据 <see cref="Exception"/> 对象返回HTTP状态码
/// </summary>
public interface IHttpExceptionStatusCodeFinder
{
    /// <summary>
    /// 获取HTTP状态码
    /// </summary>
    /// <param name="context">HTTP请求上下文</param>
    /// <param name="exception"><see cref="Exception"/> 对象</param>
    /// <returns></returns>
    HttpStatusCode GetStatusCode(HttpContext context, Exception exception);
}