using System.ComponentModel.DataAnnotations;

namespace NoelleNet.AspNetCore.Validation;

/// <summary>
/// 实现该接口的类会有一个ValidationResults属性
/// </summary>
public interface IHasValidationResults
{
    /// <summary>
    /// 模型验证结果集
    /// </summary>
    IEnumerable<ValidationResult> ValidationResults { get; }
}
