using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;
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

        // 遍历所有参数
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument == null)
                continue;

            // 获取该参数类型的IValidator的实例对象
            Type validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
                continue;

            // 创建IValidationContext实例
            Type validationContextType = typeof(ValidationContext<>).MakeGenericType(argument.GetType());
            IValidationContext? validationContext = Activator.CreateInstance(validationContextType, argument) as IValidationContext;

            // 验证模型，并获取验证结果
            var validationResult = await validator.ValidateAsync(validationContext);
            if (validationResult.IsValid)
                continue;

            results.AddRange(validationResult.Errors.Select(e => new ValidationResult(e.ErrorMessage, [e.PropertyName])));
        }

        if (results.Count > 0)
            throw new NoelleValidationException(results);

        await next();
    }
}
