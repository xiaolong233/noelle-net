using NoelleNet.Core.Exceptions;
using NoelleNet.Validation;
using System.Net;

namespace NoelleNet.AspNetCore.ExceptionHandling;

/// <summary>
/// 异常的HTTP状态码发现者的默认实现
/// </summary>
public class NoelleHttpExceptionStatusCodeFinder : IHttpExceptionStatusCodeFinder
{
    /// <summary>
    /// 获取HTTP状态码
    /// </summary>
    /// <param name="context">HTTP请求上下文</param>
    /// <param name="exception">异常对象</param>
    /// <returns></returns>
    public virtual HttpStatusCode GetStatusCode(HttpContext context, Exception exception)
    {
        if (exception is IHasHttpStatusCode statusCodeException)
            return (HttpStatusCode)statusCodeException.StatusCode;
        if (exception is NoelleNotFoundException)
            return HttpStatusCode.NotFound;
        if (exception is System.ComponentModel.DataAnnotations.ValidationException || exception is IHasValidationResults)
            return HttpStatusCode.BadRequest;
        if (exception is NoelleConflictException)
            return HttpStatusCode.Conflict;
        if (exception is NoelleBusinessException)
            return HttpStatusCode.Forbidden;
        return HttpStatusCode.InternalServerError;
    }
}
