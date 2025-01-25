using Microsoft.AspNetCore.Mvc.Filters;
using NoelleNet.Validation;
using System.ComponentModel.DataAnnotations;

namespace NoelleNet.AspNetCore.Validation;

/// <summary>
/// 基于 ModelState 的模型自动验证的筛选器
/// </summary>
public class NoelleModelValidationFilter : IAsyncActionFilter
{
    /// <summary>
    /// 在操作之前、模型绑定完成后异步调用
    /// </summary>
    /// <param name="context">操作筛选器的上下文</param>
    /// <param name="next">调用以执行下一个操作筛选器或操作本身</param>
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

                results.AddRange(entity.Errors.Select(e => new ValidationResult(e.ErrorMessage, [name])));
            }

            throw new NoelleValidationException(results);
        }

        await next();
    }
}