using System.ComponentModel.DataAnnotations;

namespace NoelleNet.Validation;

/// <summary>
/// 实现该接口的类会有一个 ValidationResults 属性
/// </summary>
public interface IHasValidationResults
{
    /// <summary>
    /// 模型验证结果集
    /// </summary>
    IEnumerable<ValidationResult> ValidationResults { get; }
}
