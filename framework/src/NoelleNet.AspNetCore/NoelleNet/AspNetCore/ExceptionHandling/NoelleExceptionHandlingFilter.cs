using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NoelleNet.Http;
using System.Net;

namespace NoelleNet.AspNetCore.ExceptionHandling;

/// <summary>
/// 全局异常处理筛选器
/// </summary>
/// <remarks>
/// 构造函数
/// </remarks>
/// <param name="converter">异常信息转换器</param>
/// <param name="finder">HTTP错误状态码查找器</param>
public class NoelleExceptionHandlingFilter(IExceptionToErrorConverter converter, IHttpExceptionStatusCodeFinder finder) : IAsyncExceptionFilter
{
    private readonly IExceptionToErrorConverter _converter = converter ?? throw new ArgumentNullException(nameof(converter));
    private readonly IHttpExceptionStatusCodeFinder _finder = finder ?? throw new ArgumentNullException(nameof(finder));

    /// <summary>
    /// 在操作引发 <see cref="Exception"/> 后调用 
    /// </summary>
    /// <param name="context">异常上下文</param>
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
            ILogger<NoelleExceptionHandlingFilter> logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<NoelleExceptionHandlingFilter>>();
            logger.LogError(new EventId(context.Exception.HResult), context.Exception, context.Exception.Message, []);
        }

        context.HttpContext.Response.StatusCode = (int)statusCode;
        context.Result = new JsonResult(new NoelleErrorResponseDto(error));

        // 标记异常已被处理
        context.ExceptionHandled = true;

        return Task.CompletedTask;
    }
}
