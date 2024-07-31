using System.ComponentModel.DataAnnotations;

namespace NoelleNet.Validation;

/// <summary>
/// 实现了 <see cref="IHasValidationResults"/> 接口的模型验证失败异常类
/// </summary>
public class NoelleValidationException : Exception, IHasValidationResults
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="message">异常信息</param>
    /// <param name="validationResults">模型验证结果集</param>
    public NoelleValidationException(string message, IEnumerable<ValidationResult> validationResults) : base(message)
    {
        ArgumentNullException.ThrowIfNull(validationResults);

        ValidationResults = validationResults;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="validationResults">模型验证结果集</param>
    public NoelleValidationException(IEnumerable<ValidationResult> validationResults) : this("发生一个或多个验证错误", validationResults)
    {
    }

    /// <summary>
    /// 模型验证结果集
    /// </summary>
    public IEnumerable<ValidationResult> ValidationResults { get; }
}