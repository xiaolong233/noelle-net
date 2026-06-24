using Microsoft.AspNetCore.Http;
using Moq;
using NoelleNet.ExceptionHandling;
using NoelleNet.Http;
using NoelleNet.Validation;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Net;

namespace NoelleNet.AspNetCore.ExceptionHandling;

public class NoelleHttpExceptionStatusCodeFinderTests
{
    private readonly NoelleHttpExceptionStatusCodeFinder _finder = new();
    private readonly Mock<HttpContext> _httpContextMock = new();

    [Fact]
    public void GetStatusCode_ExceptionWithStatusCode_ShouldReturnStatusCode()
    {
        var exception = new NoelleRemoteCallException { StatusCode = 404 };

        var result = _finder.GetStatusCode(_httpContextMock.Object, exception);

        Assert.Equal(HttpStatusCode.NotFound, result);
    }

    [Fact]
    public void GetStatusCode_BusinessException_ShouldReturnForbidden()
    {
        var exception = new BusinessException();

        var result = _finder.GetStatusCode(_httpContextMock.Object, exception);

        Assert.Equal(HttpStatusCode.Forbidden, result);
    }

    [Fact]
    public void GetStatusCode_EntityNotFoundException_ShouldReturnNotFound()
    {
        var exception = new EntityNotFoundException("not found");

        var result = _finder.GetStatusCode(_httpContextMock.Object, exception);

        Assert.Equal(HttpStatusCode.NotFound, result);
    }

    [Fact]
    public void GetStatusCode_DataAnnotationValidationException_ShouldReturnBadRequest()
    {
        var exception = new ValidationException("validation failed");

        var result = _finder.GetStatusCode(_httpContextMock.Object, exception);

        Assert.Equal(HttpStatusCode.BadRequest, result);
    }

    [Fact]
    public void GetStatusCode_NoelleValidationException_ShouldReturnBadRequest()
    {
        var exception = new NoelleValidationException([]);

        var result = _finder.GetStatusCode(_httpContextMock.Object, exception);

        Assert.Equal(HttpStatusCode.BadRequest, result);
    }

    [Fact]
    public void GetStatusCode_DBConcurrencyException_ShouldReturnConflict()
    {
        var exception = new DBConcurrencyException();

        var result = _finder.GetStatusCode(_httpContextMock.Object, exception);

        Assert.Equal(HttpStatusCode.Conflict, result);
    }

    [Fact]
    public void GetStatusCode_NotImplementedException_ShouldReturnNotImplemented()
    {
        var exception = new NotImplementedException();

        var result = _finder.GetStatusCode(_httpContextMock.Object, exception);

        Assert.Equal(HttpStatusCode.NotImplemented, result);
    }

    [Fact]
    public void GetStatusCode_GenericException_ShouldReturnInternalServerError()
    {
        var exception = new Exception();

        var result = _finder.GetStatusCode(_httpContextMock.Object, exception);

        Assert.Equal(HttpStatusCode.InternalServerError, result);
    }

    [Fact]
    public void GetStatusCode_IHasHttpStatusCode_ZeroStatus_ShouldFallThrough()
    {
        var exception = new NoelleRemoteCallException { StatusCode = 0 };

        var result = _finder.GetStatusCode(_httpContextMock.Object, exception);

        Assert.Equal(HttpStatusCode.InternalServerError, result);
    }

    [Fact]
    public void GetStatusCode_IHasHttpStatusCode_PriorityOverBusinessException()
    {
        var exception = new CustomStatusCodeException("not found error", 404);

        var result = _finder.GetStatusCode(_httpContextMock.Object, exception);

        Assert.Equal(HttpStatusCode.NotFound, result);
    }

    private class CustomStatusCodeException(string message, int statusCode) : Exception(message), IHasHttpStatusCode
    {
        public int StatusCode { get; } = statusCode;
    }
}
