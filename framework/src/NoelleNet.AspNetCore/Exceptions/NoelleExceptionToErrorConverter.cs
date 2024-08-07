﻿using NoelleNet.AspNetCore.ErrorHandling;
using NoelleNet.Core.Exceptions;
using NoelleNet.Validation;

namespace NoelleNet.AspNetCore.Exceptions;

/// <summary>
/// 提供默认的实现此接口以将 <see cref="Exception"/> 对象转换为 <see cref="NoelleErrorDetailDto"/> 对象。
/// </summary>
public class NoelleExceptionToErrorConverter : IExceptionToErrorConverter
{
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
    /// 创建 <see cref="NoelleErrorDetailDto"/> 对象
    /// </summary>
    /// <param name="exception"><see cref="Exception"/> 对象</param>
    /// <returns></returns>
    protected virtual NoelleErrorDetailDto CreateError(Exception exception)
    {
        if (exception is System.ComponentModel.DataAnnotations.ValidationException validationException)
        {
            return CreateValidationFailError(validationException);
        }

        if (exception is IHasValidationResults validationResults)
        {
            return CreateValidationFailError(validationResults);
        }

        if (exception is NoelleNotFoundException notFoundException)
        {
            return CreateNotFoundError(notFoundException);
        }

        return new NoelleErrorDetailDto("发生意外错误")
        {
            Code = NoelleConstants.ErrorCodes.InternalServerError,
            Details = [new NoelleErrorDetailDto(exception.Message)],
            Extra = exception.Data
        };
    }

    /// <summary>
    /// 创建模型验证失败的 <see cref="NoelleErrorDetailDto"/> 对象
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    private static NoelleErrorDetailDto CreateValidationFailError(System.ComponentModel.DataAnnotations.ValidationException exception)
    {
        var errors = exception.ValidationResult.MemberNames
            .Select(m => new NoelleErrorDetailDto(exception.ValidationResult.ErrorMessage ?? string.Empty) { Target = m });

        return new NoelleErrorDetailDto("发生一个或多个验证错误。")
        {
            Code = NoelleConstants.ErrorCodes.ValidationFailed,
            Details = errors
        };
    }

    /// <summary>
    /// 创建模型验证失败的 <see cref="NoelleErrorDetailDto"/> 对象
    /// </summary>
    /// <param name="validationResults">实现了 <see cref="IHasValidationResults"/> 对象实例</param>
    /// <returns></returns>
    private static NoelleErrorDetailDto CreateValidationFailError(IHasValidationResults exception)
    {
        return new NoelleErrorDetailDto("发生一个或多个验证错误。")
        {
            Code = NoelleConstants.ErrorCodes.ValidationFailed,
            Details = exception.ValidationResults.SelectMany(s => s.MemberNames.Select(m => new NoelleErrorDetailDto(s.ErrorMessage ?? string.Empty) { Target = m }))
        };
    }

    /// <summary>
    /// 创建资源未找到的 <see cref="NoelleErrorDetailDto"/> 对象
    /// </summary>
    /// <param name="exception"><see cref="NoelleEntityNotFoundException"/> 对象</param>
    /// <returns></returns>
    private static NoelleErrorDetailDto CreateNotFoundError(NoelleNotFoundException exception)
    {
        return new NoelleErrorDetailDto(exception.Message) { Code = NoelleConstants.ErrorCodes.NotFound };
    }
}
