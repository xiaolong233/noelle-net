﻿using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;
using System.ComponentModel.DataAnnotations;

namespace NoelleNet.AspNetCore.Validation;

/// <summary>
/// 基于FluentValidation的模型自动验证实现的筛选器
/// </summary>
public class FluentValidationFilter : IAsyncActionFilter
{
    /// <summary>
    /// 处理动作执行
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // 遍历Action的参数
        List<ValidationResult> results = [];
        foreach (var parameter in context.ActionDescriptor.Parameters)
        {
            // 获取该参数类型的IValidator的实例对象
            Type validatorType = typeof(IValidator<>).MakeGenericType(parameter.ParameterType);
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
                continue;

            // 获取参数值
            var value = context.ActionArguments[parameter.Name];

            // 创建IValidationContext实例
            Type validationContextType = typeof(ValidationContext<>).MakeGenericType(parameter.ParameterType);
            IValidationContext? validationContext = Activator.CreateInstance(validationContextType, value) as IValidationContext;

            // 验证模型，并获取验证结果
            var validationResult = await validator.ValidateAsync(validationContext);
            if (validationResult.IsValid)
                continue;

            results.AddRange(validationResult.Errors.Select(e => new ValidationResult(e.ErrorMessage, new string[] { e.PropertyName })));
        }

        if (results.Count > 0)
            throw new NoelleNet.AspNetCore.Validation.ValidationException(results);

        await next();
    }
}
