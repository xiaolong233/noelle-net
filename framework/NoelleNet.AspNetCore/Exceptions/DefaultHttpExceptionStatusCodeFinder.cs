using NoelleNet.AspNetCore.Validation;
using NoelleNet.Ddd.Domain.Exceptions;
using System.Net;

namespace NoelleNet.AspNetCore.Exceptions;

/// <summary>
/// 异常的HTTP状态码发现者的默认实现
/// </summary>
public class DefaultHttpExceptionStatusCodeFinder : IHttpExceptionStatusCodeFinder
{
    /// <summary>
    /// 获取HTTP状态码
    /// </summary>
    /// <param name="context">HTTP请求上下文</param>
    /// <param name="exception">异常对象</param>
    /// <returns></returns>
    public virtual HttpStatusCode GetStatusCode(HttpContext context, Exception exception)
    {
        if (exception is IHasHttpStatusCode hasHttpStatusCodeException)
            return (HttpStatusCode)hasHttpStatusCodeException.HttpStatusCode;
        if (exception is EntityNotFoundException)
            return HttpStatusCode.NotFound;
        if (exception is System.ComponentModel.DataAnnotations.ValidationException || exception is IHasValidationResults)
            return HttpStatusCode.BadRequest;
        return HttpStatusCode.InternalServerError;
    }
}
