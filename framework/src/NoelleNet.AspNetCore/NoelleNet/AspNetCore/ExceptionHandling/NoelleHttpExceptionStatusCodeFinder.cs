using NoelleNet.ExceptionHandling;
using NoelleNet.Validation;
using System.Data;
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
        if (exception is IHasHttpStatusCode statusCodeException && statusCodeException.StatusCode > 0)
            return (HttpStatusCode)statusCodeException.StatusCode;
        if (exception is IBusinessException)
            return HttpStatusCode.Forbidden;
        if (exception is EntityNotFoundException)
            return HttpStatusCode.NotFound;
        if (exception is System.ComponentModel.DataAnnotations.ValidationException || exception is IHasValidationResults)
            return HttpStatusCode.BadRequest;
        if (exception is DBConcurrencyException)
            return HttpStatusCode.Conflict;
        if (exception is NotImplementedException)
            return HttpStatusCode.NotImplemented;
        return HttpStatusCode.InternalServerError;
    }
}
