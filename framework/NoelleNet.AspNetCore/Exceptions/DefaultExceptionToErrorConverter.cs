using NoelleNet.AspNetCore.Validation;
using NoelleNet.Ddd.Domain.Exceptions;

namespace NoelleNet.AspNetCore.Exceptions;

/// <summary>
/// 提供默认的实现此接口以将 <see cref="Exception"/> 对象转换为 <see cref="Error"/> 对象。
/// </summary>
public class DefaultExceptionToErrorConverter : IExceptionToErrorConverter
{
    /// <summary>
    /// 转换处理
    /// </summary>
    /// <param name="exception">需要进行转换处理的异常对象</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Error Covert(Exception exception)
    {
        Error error = CreateError(exception);
        if (exception is IHasErrorCode hasErrorCode)
            error.Code = hasErrorCode.ErrorCode;

        return error;
    }

    /// <summary>
    /// 创建 <see cref="Error"/> 对象
    /// </summary>
    /// <param name="exception"><see cref="Exception"/> 对象</param>
    /// <returns></returns>
    protected virtual Error CreateError(Exception exception)
    {
        if (exception is System.ComponentModel.DataAnnotations.ValidationException validationException)
        {
            return CreateValidationFailError(validationException);
        }

        if (exception is IHasValidationResults validationResults)
        {
            return CreateValidationFailError(validationResults);
        }

        if (exception is EntityNotFoundException notFoundException)
        {
            return CreateNotFoundError(notFoundException);
        }

        return new Error(exception.Message)
        {
            Extra = exception.Data
        };
    }

    /// <summary>
    /// 创建模型验证失败的 <see cref="Error"/> 对象
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    private Error CreateValidationFailError(System.ComponentModel.DataAnnotations.ValidationException exception)
    {
        var errors = exception.ValidationResult.MemberNames
            .Select(m => new ValidationFailureError(exception.ValidationResult.ErrorMessage ?? string.Empty, m));

        return new Error("发生一个或多个验证错误。")
        {
            Code = ConstantTable.ErrorCodes.ValidationFailed,
            Details = errors
        };
    }

    /// <summary>
    /// 创建模型验证失败的 <see cref="Error"/> 对象
    /// </summary>
    /// <param name="validationResults">实现了 <see cref="IHasValidationResults"/> 对象实例</param>
    /// <returns></returns>
    private Error CreateValidationFailError(IHasValidationResults exception)
    {
        return new Error("发生一个或多个验证错误。")
        {
            Code = ConstantTable.ErrorCodes.ValidationFailed,
            Details = exception.ValidationResults.SelectMany(s => s.MemberNames.Select(m => new ValidationFailureError(s.ErrorMessage ?? string.Empty, m)))
        };
    }

    /// <summary>
    /// 创建资源未找到的 <see cref="Error"/> 对象
    /// </summary>
    /// <param name="exception"><see cref="EntityNotFoundException"/> 对象</param>
    /// <returns></returns>
    private Error CreateNotFoundError(EntityNotFoundException exception)
    {
        return new Error(exception.Message) { Code = ConstantTable.ErrorCodes.NotFound };
    }
}
