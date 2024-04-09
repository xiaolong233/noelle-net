using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace NoelleNet.AspNetCore.Exceptions;

/// <summary>
/// 全局异常处理筛选器
/// </summary>
public class GlobalExceptionHandleFilter : IAsyncExceptionFilter
{
    private readonly IExceptionToErrorConverter _converter;
    private readonly IHttpExceptionStatusCodeFinder _finder;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="converter"></param>
    /// <param name="finder"></param>
    public GlobalExceptionHandleFilter(IExceptionToErrorConverter converter, IHttpExceptionStatusCodeFinder finder)
    {
        _converter = converter;
        _finder = finder;
    }

    /// <summary>
    /// 处理异常
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public Task OnExceptionAsync(ExceptionContext context)
    {
        // 把异常对象转换成错误信息对象
        Error error = _converter.Covert(context.Exception);

        // 根据异常获取对应的HTTP状态码
        HttpStatusCode statusCode = _finder.GetStatusCode(context.HttpContext, context.Exception);

        // 记录服务器内部错误
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            ILogger<GlobalExceptionHandleFilter> logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<GlobalExceptionHandleFilter>>();
            logger.LogError(new EventId(context.Exception.HResult), context.Exception, context.Exception.Message);
        }

        context.HttpContext.Response.StatusCode = (int)statusCode;
        context.Result = new JsonResult(new ErrorResponse(error));

        // 标记异常已被处理
        context.ExceptionHandled = true;

        return Task.CompletedTask;
    }
}
