using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using NoelleNet.AspNetCore.ExceptionHandling.Localization;
using NoelleNet.ExceptionHandling;
using NoelleNet.Http;
using NoelleNet.Validation;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Text;

namespace NoelleNet.AspNetCore.ExceptionHandling;

/// <summary>
/// <see cref="IExceptionToErrorConverter"/> 的默认实现
/// </summary>
public class NoelleExceptionToErrorConverter : IExceptionToErrorConverter
{
    private readonly IStringLocalizer<NoelleExceptionHandlingResource> _localizer;
    private readonly IStringLocalizerFactory _localizerFactory;
    private readonly IOptions<NoelleExceptionLocalizationOptions> _localizationOptions;

    /// <summary>
    /// 创建一个新的 <see cref="NoelleExceptionToErrorConverter"/> 实例
    /// </summary>
    /// <param name="localizer">字符串本地化器</param>
    /// <param name="localizerFactory">字符串本地化工厂</param>
    /// <param name="localizationOptions">本地化选项</param>
    /// <exception cref="ArgumentNullException"></exception>
    public NoelleExceptionToErrorConverter(
        IStringLocalizer<NoelleExceptionHandlingResource> localizer,
        IStringLocalizerFactory localizerFactory,
        IOptions<NoelleExceptionLocalizationOptions> localizationOptions)
    {
        _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        _localizerFactory = localizerFactory ?? throw new ArgumentNullException(nameof(localizerFactory));
        _localizationOptions = localizationOptions ?? throw new ArgumentNullException(nameof(localizationOptions));
    }

    /// <inheritdoc />
    public RemoteCallErrorInfo Covert(Exception exception, Action<NoelleExceptionHandlingOptions>? optionsBuilder = null)
    {
        var options = CreateDefaultOptions();
        optionsBuilder?.Invoke(options);

        RemoteCallErrorInfo error = CreateErrorInfo(exception, options);

        // 处理错误代码
        if (exception is IHasErrorCode hasErrorCode && !string.IsNullOrWhiteSpace(hasErrorCode.ErrorCode))
            error.Code = hasErrorCode.ErrorCode;

        return error;
    }

    /// <summary>
    /// 根据传入的异常创建相应的错误详情对象
    /// </summary>
    /// <param name="e">异常实例</param>
    /// <param name="options">异常信息转换选项</param>
    /// <returns></returns>
    protected virtual RemoteCallErrorInfo CreateErrorInfo(Exception e, NoelleExceptionHandlingOptions options)
    {
        RemoteCallErrorInfo errorInfo;

        if (e is NoelleRemoteCallException remoteCallException && remoteCallException.Error != null)
        {
            errorInfo = remoteCallException.Error;
        }
        else if (e is DBConcurrencyException)
        {
            errorInfo = new RemoteCallErrorInfo(_localizer["DBConcurrencyErrorMessage"]);
        }
        else if (e is EntityNotFoundException entityNotFoundException)
        {
            errorInfo = CreateEntityNotFoundError(entityNotFoundException);
        }
        else
        {
            errorInfo = new RemoteCallErrorInfo(e.Message);

            if (e is IHasValidationResults validationException)
            {
                if (string.IsNullOrWhiteSpace(errorInfo.Message))
                {
                    errorInfo.Message = _localizer["ValidationFailedErrorMessage"];
                }

                if (!options.IncludeExceptionDetails)
                {
                    errorInfo.Details = GetValidationErrorDetails(validationException);
                }

                errorInfo.ValidationErrors = validationException.ValidationResults.Select(CreateValidationError);
            }

            if (string.IsNullOrWhiteSpace(errorInfo.Message))
                errorInfo.Message = TryGetLocalizedMessage(e);

            if (string.IsNullOrWhiteSpace(errorInfo.Message))
                errorInfo.Message = _localizer["InternalServerErrorMessage"];
        }

        if (options.IncludeExceptionDetails)
        {
            StringBuilder detailsBuilder = new StringBuilder();
            AddExceptionToDetails(e, detailsBuilder, options.IncludeStackTrace);
            errorInfo.Details = detailsBuilder.ToString();
        }

        if (options.IncludeExceptionData)
        {
            errorInfo.Data = e.Data;
        }

        return errorInfo;
    }

    /// <summary>
    /// 创建未找到实体的错误信息
    /// </summary>
    /// <param name="e"><see cref="EntityNotFoundException"/> 实例</param>
    /// <returns></returns>
    protected virtual RemoteCallErrorInfo CreateEntityNotFoundError(EntityNotFoundException e)
    {
        string message = e.Message;

        if (e.EntityType != null)
        {
            if (e.Id == null)
            {
                message = _localizer["EntityNotFoundErrorMessageWithoutId", e.EntityType.Name];
            }
            else
            {
                message = _localizer["EntityNotFoundErrorMessage", e.Id, e.EntityType.Name];
            }
        }

        return new RemoteCallErrorInfo(message);
    }

    /// <summary>
    /// 创建 <see cref="RemoteCallValidationErrorInfo"/> 实例
    /// </summary>
    /// <param name="validationResult"><see cref="ValidationResult"/> 实例</param>
    /// <returns></returns>
    protected virtual RemoteCallValidationErrorInfo CreateValidationError(ValidationResult validationResult)
    {
        return new RemoteCallValidationErrorInfo(validationResult.ErrorMessage!, validationResult.MemberNames);
    }

    /// <summary>
    /// 获取模型验证错误详情
    /// </summary>
    /// <param name="e"><see cref="IHasValidationResults"/> 实例</param>
    /// <returns></returns>
    protected virtual string GetValidationErrorDetails(IHasValidationResults e)
    {
        var sb = new StringBuilder();

        sb.AppendLine(_localizer["ValidationErrorDetailsTitle"]);

        foreach (var result in e.ValidationResults)
        {
            sb.AppendLine($" - {result.ErrorMessage}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// 获取本地化的错误消息
    /// </summary>
    /// <param name="e"><see cref="Exception"/> 实例</param>
    /// <returns></returns>
    protected virtual string? TryGetLocalizedMessage(Exception e)
    {
        if (e is not IHasErrorCode errorCodeException)
            return null;

        if (string.IsNullOrWhiteSpace(errorCodeException.ErrorCode))
            return null;

        var localizer = _localizationOptions.Value.LocalizerProvider?.Invoke(e, _localizerFactory);
        if (localizer == null)
            return default;

        LocalizedString localizedString = localizer[errorCodeException.ErrorCode];
        if (localizedString.ResourceNotFound)
            return null;

        string message = localizedString.Value;
        if (e.Data != null && e.Data.Count > 0)
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
    /// 添加异常详情到错误详情中
    /// </summary>
    /// <param name="e"><see cref="Exception"/> 实例</param>
    /// <param name="detailsBuilder">错误详情构建器</param>
    /// <param name="includeStackTrace">包含堆栈跟踪信息</param>
    protected virtual void AddExceptionToDetails(Exception e, StringBuilder detailsBuilder, bool includeStackTrace)
    {
        detailsBuilder.AppendLine($"{e.GetType().Name}: {e.Message}");

        if (e is IHasErrorDetails hasErrorDetails && !string.IsNullOrWhiteSpace(hasErrorDetails.Details))
        {
            detailsBuilder.AppendLine(hasErrorDetails.Details);
        }

        if (e is IHasValidationResults validationException && validationException.ValidationResults.Any())
        {
            detailsBuilder.AppendLine(GetValidationErrorDetails(validationException));
        }

        if (includeStackTrace)
        {
            detailsBuilder.AppendLine($"Stack Trace: {e.StackTrace}");
        }

        if (e.InnerException != null)
        {
            AddExceptionToDetails(e.InnerException, detailsBuilder, includeStackTrace);
        }

        if (e is AggregateException aggregateException && aggregateException.InnerExceptions.Any())
        {
            foreach (var InnerException in aggregateException.InnerExceptions)
            {
                AddExceptionToDetails(InnerException, detailsBuilder, includeStackTrace);
            }
        }
    }

    /// <summary>
    /// 创建默认选项的实例
    /// </summary>
    /// <returns></returns>
    protected virtual NoelleExceptionHandlingOptions CreateDefaultOptions()
    {
        return new NoelleExceptionHandlingOptions
        {
            TraceIdProvider = () => Guid.NewGuid().ToString(),
            IncludeExceptionDetails = false,
            IncludeStackTrace = true
        };
    }
}
