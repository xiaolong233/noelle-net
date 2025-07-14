using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using NoelleNet.AspNetCore.Validation.Localization;
using NoelleNet.Validation;
using System.ComponentModel.DataAnnotations;

namespace NoelleNet.AspNetCore.Validation;

/// <summary>
/// 基于 FluentValidation 的模型自动验证的筛选器
/// </summary>
public class NoelleFluentValidationFilter : IAsyncActionFilter
{
    /// <summary>
    /// 在操作之前、模型绑定完成后异步调用
    /// </summary>
    /// <param name="context">操作筛选器的上下文</param>
    /// <param name="next">调用以执行下一个操作筛选器或操作本身</param>
    /// <returns></returns>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        List<ValidationResult> results = [];

        // 预验证
        if (PreValidate(context, results))
        {
            // 遍历所有参数
            foreach (var parameter in context.ActionDescriptor.Parameters)
            {
                // 获取该参数类型的IValidator的实例对象
                Type validatorType = typeof(IValidator<>).MakeGenericType(parameter.ParameterType);
                if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
                    continue;

                // 获取参数值
                context.ActionArguments.TryGetValue(parameter.Name, out object? value);

                // 创建IValidationContext实例
                Type validationContextType = typeof(ValidationContext<>).MakeGenericType(parameter.ParameterType);
                IValidationContext? validationContext = Activator.CreateInstance(validationContextType, value) as IValidationContext;

                // 验证模型，并获取验证结果
                var validationResult = await validator.ValidateAsync(validationContext);
                if (validationResult.IsValid)
                    continue;

                results.AddRange(validationResult.Errors.Select(e => new ValidationResult(e.ErrorMessage, [e.PropertyName])));
            }
        }

        if (results.Count > 0)
            throw new NoelleValidationException(results);

        await next();
    }

    /// <summary>
    /// 预验证
    /// </summary>
    /// <param name="context">操作筛选器的上下文</param>
    /// <param name="results">验证结果集合</param>
    /// <returns></returns>
    protected virtual bool PreValidate(ActionExecutingContext context, List<ValidationResult> results)
    {
        var localizer = context.HttpContext.RequestServices.GetRequiredService<IStringLocalizer<NoelleValidationResource>>();

        foreach (var parameter in context.ActionDescriptor.Parameters)
        {
            context.ActionArguments.TryGetValue(parameter.Name, out object? value);
            if (value != null || Nullable.GetUnderlyingType(parameter.ParameterType) != null)
                continue;

            results.Add(new ValidationResult(string.Format(localizer["ParameterRequiredErrorMessage"], parameter.Name), [parameter.Name]));
        }

        if (results.Count > 0)
        {
            results.Insert(0, new ValidationResult(localizer["RequestBodyRequiredErrorMessage"], [string.Empty]));
        }

        return results.Count <= 0;
    }
}
