using Microsoft.AspNetCore.Mvc.Filters;
using System.ComponentModel.DataAnnotations;

namespace NoelleNet.AspNetCore.Validation;

/// <summary>
/// 基于 ModelState 的模型验证
/// </summary>
public class ModelValidationFilter : IAsyncActionFilter
{
    /// <summary>
    /// 处理动作执行
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ModelState.IsValid)
        {
            List<ValidationResult> results = [];
            foreach (var name in context.ModelState.Keys)
            {
                var entity = context.ModelState[name];
                if (entity == null)
                    continue;

                results.AddRange(entity.Errors.Select(e => new ValidationResult(e.ErrorMessage, new string[] { name })));
            }

            throw new NoelleNet.AspNetCore.Validation.ValidationException(results);
        }

        await next();
    }
}