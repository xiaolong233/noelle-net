using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NoelleNet.AspNetCore.Mvc;

/// <summary>
/// 根据 <see cref="IActionResult"/> 和 HTTP 方法 设置 HTTP状态码
/// </summary>
public class NoelleActionResultStatusCodeFilter : IAsyncResultFilter
{
    /// <summary>
    /// 在操作结果之前异步调用
    /// </summary>
    /// <param name="context">结果筛选器的上下文</param>
    /// <param name="next">调用以执行下一个结果筛选器或结果本身</param>
    /// <returns></returns>
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        string method = context.HttpContext.Request.Method;
        if (HttpMethods.IsGet(method))
        {
            HandleGet(context);
        }
        else if (HttpMethods.IsPost(method))
        {
            HandlePost(context);
        }
        else if (HttpMethods.IsPut(method))
        {
            HandlePut(context);
        }
        else if (HttpMethods.IsPatch(method))
        {
            HandlePatch(context);
        }
        else if (HttpMethods.IsDelete(method))
        {
            HandleDelete(context);
        }
        else if (HttpMethods.IsConnect(method))
        {
            HandleConnect(context);
        }
        else if (HttpMethods.IsHead(method))
        {
            HandleHead(context);
        }
        else if (HttpMethods.IsOptions(method))
        {
            HandleOptions(context);
        }
        else if (HttpMethods.IsTrace(method))
        {
            HandleTrace(context);
        }

        await next();
    }

    /// <summary>
    /// 处理 GET 请求返回的状态码
    /// </summary>
    /// <param name="context"></param>
    protected virtual void HandleGet(ResultExecutingContext context)
    {
        if (context.Result is EmptyResult)
        {
            context.Result = new NoContentResult();
        }
    }

    /// <summary>
    /// 处理 POST 请求返回的状态码
    /// </summary>
    /// <param name="context"></param>
    protected virtual void HandlePost(ResultExecutingContext context)
    {
        if (context.Result is EmptyResult)
        {
            context.Result = new NoContentResult();
        }
        else if (context.Result is ObjectResult objectResult)
        {
            if (objectResult.StatusCode == null)
            {
                context.Result = new JsonResult(objectResult.Value) { StatusCode = StatusCodes.Status201Created };
            }
        }
    }

    /// <summary>
    /// 处理 PUT 请求返回的状态码
    /// </summary>
    /// <param name="context"></param>
    protected virtual void HandlePut(ResultExecutingContext context)
    {
        if (context.Result is EmptyResult)
        {
            context.Result = new NoContentResult();
        }
        else if (context.Result is ObjectResult objectResult)
        {
            if (objectResult.StatusCode == null)
            {
                context.Result = new OkObjectResult(objectResult.Value);
            }
        }
    }

    /// <summary>
    /// 处理 PATCH 请求返回的状态码
    /// </summary>
    /// <param name="context"></param>
    protected virtual void HandlePatch(ResultExecutingContext context)
    {
        if (context.Result is EmptyResult)
        {
            context.Result = new NoContentResult();
        }
        else if (context.Result is ObjectResult objectResult)
        {
            if (objectResult.StatusCode == null)
            {
                context.Result = new OkObjectResult(objectResult.Value);
            }
        }
    }

    /// <summary>
    /// 处理 DELETE 请求返回的状态码
    /// </summary>
    /// <param name="context"></param>
    protected virtual void HandleDelete(ResultExecutingContext context)
    {
        if (context.Result is EmptyResult)
        {
            context.Result = new NoContentResult();
        }
        else if (context.Result is ObjectResult objectResult)
        {
            if (objectResult.StatusCode == null)
            {
                context.Result = new OkObjectResult(objectResult.Value);
            }
        }
    }

    /// <summary>
    /// 处理 CONNECT 请求返回的状态码
    /// </summary>
    /// <param name="context"></param>
    protected virtual void HandleConnect(ResultExecutingContext context)
    {
    }

    /// <summary>
    /// 处理 HEAD 请求返回的状态码
    /// </summary>
    /// <param name="context"></param>
    protected virtual void HandleHead(ResultExecutingContext context)
    {
    }

    /// <summary>
    /// 处理 OPTIONS 请求返回的状态码
    /// </summary>
    /// <param name="context"></param>
    protected virtual void HandleOptions(ResultExecutingContext context)
    {
    }

    /// <summary>
    /// 处理 TRACE 请求返回的状态码
    /// </summary>
    /// <param name="context"></param>
    protected virtual void HandleTrace(ResultExecutingContext context)
    {
    }
}
