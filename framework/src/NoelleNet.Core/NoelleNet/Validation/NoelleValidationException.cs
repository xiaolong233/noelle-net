using Microsoft.Extensions.Logging;
using NoelleNet.Logging;
using System.ComponentModel.DataAnnotations;

namespace NoelleNet.Validation;

/// <summary>
/// 实现了 <see cref="IHasValidationResults"/> 接口的模型验证失败异常类
/// </summary>
public class NoelleValidationException : Exception, IHasValidationResults, IHasLogLevel
{
    /// <summary>
    /// 创建一个新的 <see cref="NoelleValidationException"/> 实例
    /// </summary>
    /// <param name="message">异常信息</param>
    /// <param name="validationResults">模型验证结果集</param>
    public NoelleValidationException(string message, IEnumerable<ValidationResult> validationResults) : base(message)
    {
        ArgumentNullException.ThrowIfNull(validationResults);

        ValidationResults = validationResults;
    }

    /// <summary>
    /// 创建一个新的 <see cref="NoelleValidationException"/> 实例
    /// </summary>
    /// <param name="validationResults">模型验证结果集</param>
    public NoelleValidationException(IEnumerable<ValidationResult> validationResults)
    {
        ArgumentNullException.ThrowIfNull(validationResults);

        ValidationResults = validationResults;
    }

    /// <inheritdoc/>
    public IEnumerable<ValidationResult> ValidationResults { get; set; }

    /// <inheritdoc/>
    public LogLevel LogLevel { get; set; } = LogLevel.Warning;
}