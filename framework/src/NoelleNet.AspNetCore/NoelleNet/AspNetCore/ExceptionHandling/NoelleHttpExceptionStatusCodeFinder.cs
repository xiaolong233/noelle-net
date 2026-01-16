using NoelleNet.ExceptionHandling;
using NoelleNet.Validation;
using System.Net;

namespace NoelleNet.AspNetCore.ExceptionHandling;

/// <summary>
/// <see cref="IHttpExceptionStatusCodeFinder"/> 的默认实现
/// </summary>
public class NoelleHttpExceptionStatusCodeFinder : IHttpExceptionStatusCodeFinder
{
    /// <inheritdoc/>
    public virtual HttpStatusCode GetStatusCode(HttpContext context, Exception exception)
    {
        if (exception is IHasHttpStatusCode statusCodeException)
            return (HttpStatusCode)statusCodeException.StatusCode;
        if (exception is NoelleBusinessException)
            return HttpStatusCode.Forbidden;
        if (exception is NoelleNotFoundException)
            return HttpStatusCode.NotFound;
        if (exception is System.ComponentModel.DataAnnotations.ValidationException || exception is IHasValidationResults)
            return HttpStatusCode.BadRequest;
        if (exception is NoelleConflictException)
            return HttpStatusCode.Conflict;
        return HttpStatusCode.InternalServerError;
    }
}
