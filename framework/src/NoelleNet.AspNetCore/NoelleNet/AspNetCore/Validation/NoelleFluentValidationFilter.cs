using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using NoelleNet.AspNetCore.Validation.Localization;
using NoelleNet.Validation;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace NoelleNet.AspNetCore.Validation;

/// <summary>
/// 基于 FluentValidation 的模型自动验证的筛选器
/// </summary>
public class NoelleFluentValidationFilter : IAsyncActionFilter
{
    private readonly IStringLocalizer<NoelleValidationResource> _localizer;
    private static readonly ConcurrentDictionary<Type, Type> _validatorTypeCache = new();

    /// <summary>
    /// 创建一个新的 <see cref="NoelleFluentValidationFilter"/> 实例
    /// </summary>
    /// <param name="localizer"><see cref="IStringLocalizer{T}"/> 实例</param>
    /// <exception cref="ArgumentNullException"></exception>
    public NoelleFluentValidationFilter(IStringLocalizer<NoelleValidationResource> localizer)
    {
        _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
    }

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
        foreach (var parameter in context.ActionDescriptor.Parameters)
        {
            if (parameter.ParameterType == typeof(CancellationToken)
                || parameter.BindingInfo?.BindingSource == BindingSource.Services
                || parameter.BindingInfo?.BindingSource == BindingSource.Special)
                continue;

            // 获取该参数类型的IValidator的实例对象
            Type validatorType = _validatorTypeCache.GetOrAdd(parameter.ParameterType, GetValidatorType);
            List<object?> validators = context.HttpContext.RequestServices.GetServices(validatorType).ToList();
            if (validators.Count <= 0)
                continue;

            // 获取参数值
            context.ActionArguments.TryGetValue(parameter.Name, out object? value);
            if (value == null)
            {
                if (parameter.BindingInfo?.EmptyBodyBehavior != EmptyBodyBehavior.Allow)
                    results.Add(new ValidationResult(_localizer["ParameterRequiredErrorMessage", parameter.Name], [parameter.Name]));
                continue;
            }

            // 验证模型，并获取验证结果
            foreach (var validator in validators)
            {
                if (validator is null)
                    continue;

                var validationContext = new ValidationContext<object>(value);
                var result = await ((IValidator)validator).ValidateAsync(validationContext, context.HttpContext.RequestAborted);
                if (result.IsValid)
                    continue;

                foreach (var error in result.Errors)
                {
                    var memberName = GetMemberName(parameter.ParameterType, error);
                    results.Add(new ValidationResult(error.ErrorMessage, [memberName]));
                }
            }
        }

        if (results.Count > 0)
            throw new NoelleValidationException(results);

        await next();
    }

    protected virtual string GetMemberName(Type parameterType, FluentValidation.Results.ValidationFailure error)
    {
        return error.PropertyName ?? string.Empty;
    }

    private static Type GetValidatorType(Type type) => typeof(IValidator<>).MakeGenericType(type);
}
