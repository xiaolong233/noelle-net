using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using NoelleNet.AspNetCore.ExceptionHandling.Localization;
using NoelleNet.ExceptionHandling;
using NoelleNet.Validation;
using System.Collections;
using System.Data;
using System.Net;

namespace NoelleNet.AspNetCore.ExceptionHandling;

/// <summary>
/// 基于 ProblemDetails [RFC9457] 规范的 <see cref="IErrorResponseWriter"/> 实现
/// </summary>
public class ProblemDetailsErrorResponseWriter : IErrorResponseWriter
{
    protected NoelleExceptionHandlingOptions ExceptionHandlingOptions { get; }
    protected NoelleExceptionLocalizationOptions LocalizationOptions { get; }
    protected IStringLocalizerFactory LocalizerFactory { get; }
    protected IStringLocalizer<NoelleExceptionHandlingResource> Localizer { get; }

    /// <summary>
    /// 创建一个新的 <see cref="ProblemDetailsErrorResponseWriter"/> 实例
    /// </summary>
    /// <param name="exceptionHandlingOptions">异常处理选项</param>
    /// <param name="localizationOptions">异常本地化选项</param>
    /// <param name="localizerFactory">本地化工厂</param>
    /// <param name="localizer">通用的异常处理本地化器</param>
    public ProblemDetailsErrorResponseWriter(
        IOptions<NoelleExceptionHandlingOptions> exceptionHandlingOptions,
        IOptions<NoelleExceptionLocalizationOptions> localizationOptions,
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<NoelleExceptionHandlingResource> localizer)
    {
        ExceptionHandlingOptions = exceptionHandlingOptions.Value;
        LocalizationOptions = localizationOptions.Value;
        LocalizerFactory = localizerFactory ?? throw new ArgumentNullException(nameof(localizerFactory));
        Localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
    }

    /// <inheritdoc/>
    public async ValueTask<bool> TryWriteAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken = default)
    {
        var problemDetailsService = httpContext.RequestServices.GetRequiredService<IProblemDetailsService>();

        var details = CreateProblemDetails(httpContext, exception);

        // 写入错误代码
        if (exception is IHasErrorCode hasErrorCode && !string.IsNullOrWhiteSpace(hasErrorCode.ErrorCode))
            details.Extensions["code"] = hasErrorCode.ErrorCode;

        // 写入异常里的额外数据
        if (ExceptionHandlingOptions.IncludeExceptionData && exception.Data.Count > 0)
            details.Extensions["data"] = GetSafeExceptionData(exception.Data);

        // 写入异常信息（用于调试）
        if (ExceptionHandlingOptions.IncludeExceptionDetails)
            details.Extensions["exception"] = GetExceptionDetails(exception);

        // 写入 HTTP 状态码
        httpContext.Response.StatusCode = details.Status ?? StatusCodes.Status500InternalServerError;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = details,
            Exception = exception,
        });
    }

    /// <summary>
    /// 根据异常类型创建一个新的 <see cref="ProblemDetails"/> 实例
    /// </summary>
    /// <param name="httpContext">当前请求的 <see cref="HttpContext"/> 上下文</param>
    /// <param name="exception">需要处理的异常实例</param>
    /// <returns></returns>
    protected virtual ProblemDetails CreateProblemDetails(HttpContext httpContext, Exception exception)
    {
        var statusCode = GetStatusCode(exception);

        ProblemDetails? details;

        if (exception is IHasValidationResults validationException)
        {
            details = CreateValidationProblemDetails(httpContext, validationException);
        }
        else if (exception is EntityNotFoundException entityNotFoundException)
        {
            details = CreateEntityNotFoundProblemDetails(httpContext, entityNotFoundException);
        }
        else if (exception is DBConcurrencyException)
        {
            details = CreateDBConcurrencyProblemDetails(httpContext, exception);
        }
        else if (exception is NotImplementedException notImplementedException)
        {
            details = CreateNotImplementedProblemDetails(httpContext, notImplementedException);
        }
        else
        {
            details = CreateDefaultProblemDetails(httpContext, exception, statusCode);
        }

        // 如果异常携带自定义错误详情，则覆盖默认的 Detail 内容
        if (exception is IHasErrorDetails hasErrorDetails && !string.IsNullOrWhiteSpace(hasErrorDetails.Details))
            details.Detail = hasErrorDetails.Details;

        details.Type = GetProblemDetailsType(statusCode);
        details.Status = (int)statusCode;

        return details;
    }

    /// <summary>
    /// 获取异常对应的 HTTP 状态码，优先使用异常自身声明的状态码
    /// </summary>
    /// <param name="exception">需要处理的异常实例</param>
    /// <returns></returns>
    protected virtual HttpStatusCode GetStatusCode(Exception exception)
    {
        if (exception is IHasHttpStatusCode statusCodeException && statusCodeException.StatusCode > 0)
            return (HttpStatusCode)statusCodeException.StatusCode;

        if (exception is IBusinessException)
            return HttpStatusCode.BadRequest;
        if (exception is System.ComponentModel.DataAnnotations.ValidationException || exception is IHasValidationResults)
            return HttpStatusCode.BadRequest;
        if (exception is DBConcurrencyException)
            return HttpStatusCode.Conflict;
        if (exception is NotImplementedException)
            return HttpStatusCode.NotImplemented;

        return HttpStatusCode.InternalServerError;
    }

    /// <summary>
    /// 创建验证错误对应的 <see cref="ProblemDetails"/> 内容
    /// </summary>
    /// <param name="httpContext">当前请求的 <see cref="HttpContext"/> 上下文</param>
    /// <param name="exception">携带验证结果的异常实例</param>
    /// <returns></returns>
    protected virtual ProblemDetails CreateValidationProblemDetails(HttpContext httpContext, IHasValidationResults exception)
    {
        var errors = new Dictionary<string, List<string>>();

        foreach (var result in exception.ValidationResults)
        {
            var members = result.MemberNames;
            if (!members.Any())
                members = [string.Empty];

            foreach (var member in members)
            {
                if (!errors.ContainsKey(member))
                    errors[member] = [];
                errors[member].Add(result.ErrorMessage ?? string.Empty);
            }
        }

        return new ProblemDetails
        {
            Title = Localizer["ValidationFailedErrorMessage"],
            Extensions =
            {
                ["errors"] = errors
            }
        };
    }

    /// <summary>
    /// 创建实体未找到错误对应的 <see cref="ProblemDetails"/> 内容
    /// </summary>
    /// <param name="httpContext">当前请求的 <see cref="HttpContext"/> 上下文</param>
    /// <param name="exception">实体未找到异常实例</param>
    /// <returns></returns>
    protected virtual ProblemDetails CreateEntityNotFoundProblemDetails(HttpContext httpContext, EntityNotFoundException exception)
    {
        // 如果异常携带实体类型信息，则在详情中附带实体 ID 和实体类型
        if (exception.EntityType != null)
        {
            var detail = exception.Id == null
                ? Localizer["EntityNotFoundErrorDetailWithoutId", exception.EntityType.Name]
                : Localizer["EntityNotFoundErrorDetail", exception.Id, exception.EntityType.Name];

            return new ProblemDetails
            {
                Title = Localizer["EntityNotFoundErrorMessage"],
                Detail = detail
            };
        }

        return new ProblemDetails
        {
            Title = Localizer["EntityNotFoundErrorMessage"]
        };
    }

    /// <summary>
    /// 创建数据库并发冲突错误对应的 <see cref="ProblemDetails"/> 内容
    /// </summary>
    /// <param name="httpContext">当前请求的 <see cref="HttpContext"/> 上下文</param>
    /// <param name="exception">并发冲突异常实例</param>
    /// <returns></returns>
    protected virtual ProblemDetails CreateDBConcurrencyProblemDetails(HttpContext httpContext, Exception exception)
    {
        return new ProblemDetails
        {
            Title = Localizer["DBConcurrencyErrorMessage"]
        };
    }

    /// <summary>
    /// 创建未实现功能错误对应的 <see cref="ProblemDetails"/> 内容
    /// </summary>
    /// <param name="httpContext">当前请求的 <see cref="HttpContext"/> 上下文</param>
    /// <param name="exception">未实现异常实例</param>
    /// <returns></returns>
    protected virtual ProblemDetails CreateNotImplementedProblemDetails(HttpContext httpContext, NotImplementedException exception)
    {
        return new ProblemDetails
        {
            Title = Localizer["NotImplementedErrorMessage"]
        };
    }

    /// <summary>
    /// 创建未匹配到特定异常类型时使用的默认 <see cref="ProblemDetails"/> 内容
    /// </summary>
    /// <param name="httpContext">当前请求的 <see cref="HttpContext"/> 上下文</param>
    /// <param name="exception">异常实例</param>
    /// <param name="statusCode">异常对应的 HTTP 状态码</param>
    /// <returns></returns>
    protected virtual ProblemDetails CreateDefaultProblemDetails(HttpContext httpContext, Exception exception, HttpStatusCode statusCode)
    {
        var details = new ProblemDetails();

        // 优先使用异常对应的本地化消息作为标题
        var localizedMessage = TryGetLocalizedMessage(exception);
        if (!string.IsNullOrWhiteSpace(localizedMessage))
            details.Title = localizedMessage;

        // 如果没有本地化消息，则根据状态码使用默认的错误消息
        if (string.IsNullOrWhiteSpace(details.Title))
            details.Title = GetDefaultErrorMessage(statusCode);

        return details;
    }

    /// <summary>
    /// 根据 HTTP 状态码获取默认的错误消息
    /// </summary>
    /// <param name="statusCode">HTTP 状态码</param>
    /// <returns></returns>
    protected virtual LocalizedString GetDefaultErrorMessage(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.BadRequest => Localizer["BadRequestErrorMessage"],
        HttpStatusCode.Unauthorized => Localizer["UnauthorizedErrorMessage"],
        HttpStatusCode.Forbidden => Localizer["ForbiddenErrorMessage"],
        HttpStatusCode.NotFound => Localizer["EntityNotFoundErrorMessage"],
        HttpStatusCode.Conflict => Localizer["DBConcurrencyErrorMessage"],
        HttpStatusCode.NotImplemented => Localizer["NotImplementedErrorMessage"],
        _ when (int)statusCode is >= 400 and < 500 => Localizer["BadRequestErrorMessage"],
        _ => Localizer["InternalServerErrorMessage"]
    };

    /// <summary>
    /// 尝试获取本地化的错误消息
    /// </summary>
    /// <param name="e"><see cref="Exception"/> 实例</param>
    /// <returns></returns>
    protected virtual string? TryGetLocalizedMessage(Exception e)
    {
        // 只有实现了 IHasErrorCode 接口且错误码非空的异常才支持本地化
        if (e is not IHasErrorCode errorCodeException)
            return null;

        if (string.IsNullOrWhiteSpace(errorCodeException.ErrorCode))
            return null;

        // 通过本地化提供程序获取针对该异常的本地化器
        var localizer = LocalizationOptions.LocalizerProvider?.Invoke(e, LocalizerFactory);
        if (localizer == null)
            return default;

        // 根据错误码获取本地化消息，未找到资源时返回 null
        LocalizedString localizedString = localizer[errorCodeException.ErrorCode];
        if (localizedString.ResourceNotFound)
            return null;

        // 使用异常数据中的键值对替换本地化消息中的占位符
        string message = localizedString.Value;
        if (e.Data.Count > 0)
        {
            foreach (var key in e.Data.Keys)
            {
                if (key == null)
                    continue;

                message = message.Replace($"{{{key}}}", e.Data[key]?.ToString() ?? string.Empty);
            }
        }

        return message;
    }

    /// <summary>
    /// 根据 HTTP 状态码获取对应的 ProblemDetails 类型标识
    /// </summary>
    /// <param name="statusCode">HTTP 状态码</param>
    /// <returns></returns>
    protected virtual string GetProblemDetailsType(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.BadRequest => NoelleProblemDetailsTypes.BadRequest,
        HttpStatusCode.Unauthorized => NoelleProblemDetailsTypes.Unauthorized,
        HttpStatusCode.Forbidden => NoelleProblemDetailsTypes.Forbidden,
        HttpStatusCode.NotFound => NoelleProblemDetailsTypes.NotFound,
        HttpStatusCode.Conflict => NoelleProblemDetailsTypes.Conflict,
        HttpStatusCode.InternalServerError => NoelleProblemDetailsTypes.InternalServerError,
        HttpStatusCode.NotImplemented => NoelleProblemDetailsTypes.NotImplemented,
        _ => NoelleProblemDetailsTypes.Default
    };

    /// <summary>
    /// 获取安全的异常数据字典
    /// </summary>
    /// <param name="data">异常携带的数据字典</param>
    /// <returns></returns>
    private static IDictionary<string, object?> GetSafeExceptionData(IDictionary data)
    {
        var result = new Dictionary<string, object?>(data.Count);

        foreach (var key in data.Keys)
        {
            if (key == null)
                continue;

            var value = data[key];
            // 安全类型的值直接保留，其他类型转换为字符串表示
            result[key.ToString()!] = IsSafeValue(value) ? value : value?.ToString();
        }

        return result;
    }

    /// <summary>
    /// 判断给定值是否为可直接序列化的安全类型
    /// </summary>
    /// <param name="value">待判断的值</param>
    /// <returns></returns>
    private static bool IsSafeValue(object? value) =>
        value is null or string or bool or char or byte or sbyte or short or ushort or int or uint
            or long or ulong or float or double or decimal or DateTime or DateTimeOffset or TimeSpan
            or Guid or DateOnly or TimeOnly;

    /// <summary>
    /// 获取异常的详细信息
    /// </summary>
    /// <param name="exception"><see cref="Exception"/> 实例</param>
    /// <returns></returns>
    private Dictionary<string, object?> GetExceptionDetails(Exception exception)
    {
        var details = new Dictionary<string, object?>
        {
            ["type"] = exception.GetType().FullName,
            ["message"] = exception.Message
        };

        // 根据配置决定是否包含堆栈跟踪信息
        if (ExceptionHandlingOptions.IncludeStackTrace && !string.IsNullOrWhiteSpace(exception.StackTrace))
            details["stackTrace"] = exception.StackTrace;

        return details;
    }
}
