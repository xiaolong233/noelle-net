using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using NoelleNet.AspNetCore.ExceptionHandling.Localization;
using NoelleNet.ExceptionHandling;
using NoelleNet.ExceptionHandling.Localization;
using NoelleNet.Http;
using NoelleNet.Validation;
using System.ComponentModel.DataAnnotations;

namespace NoelleNet.AspNetCore.ExceptionHandling;

/// <summary>
/// 提供默认的实现此接口以将 <see cref="Exception"/> 对象转换为 <see cref="NoelleErrorDetailDto"/> 对象。
/// </summary>
public class NoelleExceptionToErrorConverter(
    IStringLocalizer<NoelleExceptionHandlingResource> localizer,
    IStringLocalizerFactory localizerFactory,
    IOptions<NoelleExceptionLocalizationOptions> localizationOptions
    ) : IExceptionToErrorConverter
{
    private readonly IStringLocalizer<NoelleExceptionHandlingResource> _localizer = localizer;
    private readonly IStringLocalizerFactory _localizerFactory = localizerFactory;
    private readonly IOptions<NoelleExceptionLocalizationOptions> _localizationOptions = localizationOptions;

    /// <summary>
    /// 转换处理
    /// </summary>
    /// <param name="exception">需要进行转换处理的异常对象</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public NoelleErrorDetailDto Covert(Exception exception)
    {
        NoelleErrorDetailDto error = CreateError(exception);
        if (exception is IHasErrorCode hasErrorCode)
            error.Code = hasErrorCode.ErrorCode;

        return error;
    }

    /// <summary>
    /// 根据传入的异常创建相应的错误详情对象。
    /// </summary>
    /// <param name="e">捕获的异常对象。</param>
    /// <returns>返回包含错误详情的 <see cref="NoelleErrorDetailDto"/> 对象。</returns>
    protected virtual NoelleErrorDetailDto CreateError(Exception e)
    {
        NoelleErrorDetailDto? detailDto = TryCreateValidationFailError(e);
        if (detailDto != null)
            return detailDto;

        detailDto = TryCreateNotFoundError(e);
        if (detailDto != null)
            return detailDto;

        detailDto = TryCreateConflictError(e);
        if (detailDto != null)
            return detailDto;

        detailDto = TryCreateRemoteCallError(e);
        if (detailDto != null)
            return detailDto;

        string message = TryGetMessageFromErrorCode(e) ?? e.Message;
        if (string.IsNullOrWhiteSpace(message))
            message = _localizer["InternalServerErrorMessage"];

        return new NoelleErrorDetailDto(message) { Code = NoelleErrorCodeConstants.InternalServerError };
    }

    /// <summary>
    /// 尝试创建验证失败错误详情对象。
    /// </summary>
    /// <param name="e">捕获的异常对象。</param>
    /// <returns>如果异常包含验证错误，返回包含错误详情的 <see cref="NoelleErrorDetailDto"/> 对象；否则返回 null。</returns>
    protected virtual NoelleErrorDetailDto? TryCreateValidationFailError(Exception e)
    {
        string message = e.Message;
        if (string.IsNullOrWhiteSpace(message))
            message = _localizer["ValidationFailedErrorMessage"];

        if (e is ValidationException validationException)
        {
            var errors = validationException.ValidationResult.MemberNames.Select(m => new NoelleErrorDetailDto(validationException.ValidationResult.ErrorMessage ?? string.Empty) { Target = m });
            var detailDto = new NoelleErrorDetailDto(message)
            {
                Code = NoelleErrorCodeConstants.ValidationFailed,
                Details = errors
            };
            return detailDto;
        }

        if (e is IHasValidationResults validationResults)
        {
            var detailDto = new NoelleErrorDetailDto(message)
            {
                Code = NoelleErrorCodeConstants.ValidationFailed,
                Details = validationResults.ValidationResults.SelectMany(s => s.MemberNames.Select(m => new NoelleErrorDetailDto(s.ErrorMessage ?? string.Empty) { Target = m }))
            };
            return detailDto;
        }

        return null;
    }

    /// <summary>
    /// 尝试创建资源未找到错误详情对象。
    /// </summary>
    /// <param name="e">捕获的异常对象。</param>
    /// <returns>如果异常是 <see cref="NoelleNotFoundException"/>，返回包含错误详情的 <see cref="NoelleErrorDetailDto"/> 对象；否则返回 null。</returns>
    protected virtual NoelleErrorDetailDto? TryCreateNotFoundError(Exception e)
    {
        if (e is not NoelleNotFoundException)
            return null;

        string message = e.Message;
        if (e is NoelleEntityNotFoundException entityNotFoundException && entityNotFoundException.EntityType != null)
            message = string.Format(_localizer["EntityNotFoundErrorMessage"], entityNotFoundException.Id, entityNotFoundException.EntityType.Name);
        else if (string.IsNullOrWhiteSpace(message))
            message = _localizer["NotFoundErrorMessage"];

        return new NoelleErrorDetailDto(message) { Code = NoelleErrorCodeConstants.NotFound };
    }

    /// <summary>
    /// 尝试创建冲突错误详情对象。
    /// </summary>
    /// <param name="e">捕获的异常对象。</param>
    /// <returns>如果异常是 <see cref="NoelleConflictException"/>，返回包含错误详情的 <see cref="NoelleErrorDetailDto"/> 对象；否则返回 null。</returns>
    protected virtual NoelleErrorDetailDto? TryCreateConflictError(Exception e)
    {
        if (e is not NoelleConflictException)
            return null;

        string message = e.Message;
        if (string.IsNullOrWhiteSpace(message))
            message = _localizer["ConflictErrorMessage"];

        return new NoelleErrorDetailDto(message) { Code = NoelleErrorCodeConstants.Conflict };
    }

    /// <summary>
    /// 尝试创建远程调用错误详情对象。
    /// </summary>
    /// <param name="e">捕获的异常对象。</param>
    /// <returns>如果异常是 <see cref="NoelleRemoteCallException"/>，返回包含错误详情的 <see cref="NoelleErrorDetailDto"/> 对象；否则返回 null。</returns>
    protected virtual NoelleErrorDetailDto? TryCreateRemoteCallError(Exception e)
    {
        if (e is not NoelleRemoteCallException remoteCallException)
            return null;

        if (remoteCallException.ErrorDetail != null)
            return remoteCallException.ErrorDetail;

        string message = e.Message;
        if (string.IsNullOrWhiteSpace(message))
            message = _localizer["RemoteCallErrorMessage"];

        return new NoelleErrorDetailDto(message) { Code = NoelleErrorCodeConstants.RemoteCallFailed };
    }

    /// <summary>
    /// 尝试从异常中的错误代码获取本地化的错误消息。
    /// </summary>
    /// <param name="e">捕获的异常对象。</param>
    /// <returns>如果成功获取到本地化的错误消息，返回该消息；否则返回空字符串。</returns>
    protected virtual string? TryGetMessageFromErrorCode(Exception e)
    {
        if (e is not IHasErrorCode errorCodeException)
            return null;

        if (string.IsNullOrWhiteSpace(errorCodeException.ErrorCode) || !errorCodeException.ErrorCode.Contains(':'))
            return null;

        string resourceSourceKey = errorCodeException.ErrorCode.Split(':')[0];
        if (!_localizationOptions.Value.ResourceSources.TryGetValue(resourceSourceKey, out Type? value))
            return null;
        Type resourceSource = value;
        if (resourceSource == null)
            return null;

        var localizer = _localizerFactory.Create(resourceSource);
        LocalizedString localizedString = localizer[errorCodeException.ErrorCode];
        if (localizedString.ResourceNotFound)
            return null;

        string localizedValue = localizedString.Value;
        if (e.Data != null && e.Data.Count > 0)
        {
            foreach (var key in e.Data.Keys)
            {
                if (key == null)
                    continue;

                localizedValue = localizedValue.Replace($"{{{key}}}", e.Data[key]?.ToString() ?? string.Empty);
            }
        }

        return localizedValue;
    }
}
