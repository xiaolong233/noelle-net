using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NoelleNet.AspNetCore.ExceptionHandling;
using NoelleNet.AspNetCore.ExceptionHandling.Models;
using System.Net;

namespace NoelleNet.AspNetCore.Filters;

/// <summary>
/// 全局异常处理筛选器
/// </summary>
/// <remarks>
/// 构造函数
/// </remarks>
/// <param name="converter"></param>
/// <param name="finder"></param>
public class NoelleExceptionFilter(IExceptionToErrorConverter converter, IHttpExceptionStatusCodeFinder finder) : IAsyncExceptionFilter
{
    private readonly IExceptionToErrorConverter _converter = converter;
    private readonly IHttpExceptionStatusCodeFinder _finder = finder;

    /// <summary>
    /// 处理异常
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public Task OnExceptionAsync(ExceptionContext context)
    {
        // 把异常对象转换成错误信息对象
        NoelleErrorDetailDto error = _converter.Covert(context.Exception);

        // 根据异常获取对应的HTTP状态码
        HttpStatusCode statusCode = _finder.GetStatusCode(context.HttpContext, context.Exception);

        // 记录服务器内部错误
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            ILogger<NoelleExceptionFilter> logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<NoelleExceptionFilter>>();
            logger.LogError(new EventId(context.Exception.HResult), context.Exception, message: context.Exception.Message, args: []);
        }

        context.HttpContext.Response.StatusCode = (int)statusCode;
        context.Result = new JsonResult(new NoelleErrorResponseDto(error));

        // 标记异常已被处理
        context.ExceptionHandled = true;

        return Task.CompletedTask;
    }
}
