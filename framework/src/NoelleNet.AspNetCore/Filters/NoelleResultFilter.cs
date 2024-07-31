using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NoelleNet.AspNetCore.Filters;

/// <summary>
/// 根据 <see cref="IActionResult"/> 和 HTTP方法 设置 HTTP状态码
/// </summary>
public class NoelleResultFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        string method = context.HttpContext.Request.Method;
        if (context.Result is ObjectResult objectResult)
        {
            if (method == HttpMethods.Get)
            {
                context.Result = new OkObjectResult(objectResult.Value);
            }
            else if (method == HttpMethods.Post || method == HttpMethods.Patch || method == HttpMethods.Put)
            {
                context.Result = new JsonResult(objectResult.Value) { StatusCode = StatusCodes.Status201Created };
            }
        }
        else if (method == HttpMethods.Delete)
        {
            context.Result = new NoContentResult();
        }

        await next();
    }
}
