using System.ComponentModel.DataAnnotations;

namespace NoelleNet.AspNetCore.Validation;

/// <summary>
/// 模型验证失败时的异常信息
/// </summary>
public class ValidationException : Exception, IHasValidationResults
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="message">异常信息</param>
    /// <param name="validationResults">模型验证结果集</param>
    public ValidationException(string message, IEnumerable<ValidationResult> validationResults) : base(message)
    {
        ArgumentNullException.ThrowIfNull(validationResults);

        ValidationResults = validationResults;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="validationResults">模型验证结果集</param>
    public ValidationException(IEnumerable<ValidationResult> validationResults) : this("发生一个或多个验证错误", validationResults)
    {
    }

    /// <summary>
    /// 模型验证结果集
    /// </summary>
    public IEnumerable<ValidationResult> ValidationResults { get; }
}
