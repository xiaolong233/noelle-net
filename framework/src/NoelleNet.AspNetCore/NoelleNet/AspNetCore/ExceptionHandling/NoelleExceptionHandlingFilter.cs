using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NoelleNet.Http;
using System.Diagnostics;
using System.Net;

namespace NoelleNet.AspNetCore.ExceptionHandling;

/// <summary>
/// 全局异常处理筛选器
/// </summary>
public class NoelleExceptionHandlingFilter : IAsyncExceptionFilter
{
    private readonly IExceptionToErrorConverter _converter;
    private readonly IHttpExceptionStatusCodeFinder _finder;

    /// <summary>
    /// 创建一个新的 <see cref="NoelleExceptionHandlingFilter"/> 实例
    /// </summary>
    /// <param name="converter">异常信息转换器</param>
    /// <param name="finder">HTTP错误状态码查找器</param>
    /// <exception cref="ArgumentNullException"></exception>
    public NoelleExceptionHandlingFilter(IExceptionToErrorConverter converter, IHttpExceptionStatusCodeFinder finder)
    {
        _converter = converter ?? throw new ArgumentNullException(nameof(converter));
        _finder = finder ?? throw new ArgumentNullException(nameof(finder));
    }

    /// <summary>
    /// 在操作引发 <see cref="Exception"/> 后调用 
    /// </summary>
    /// <param name="context">异常上下文</param>
    /// <returns></returns>
    public Task OnExceptionAsync(ExceptionContext context)
    {
        // 把异常对象转换成错误信息对象
        NoelleErrorDto error = _converter.Covert(context.Exception, options =>
        {
            options.TraceIdProvider = () => context.HttpContext.TraceIdentifier;
        });

        // 根据异常获取对应的HTTP状态码
        HttpStatusCode statusCode = _finder.GetStatusCode(context.HttpContext, context.Exception);

        // 记录服务器内部错误
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            ILogger<NoelleExceptionHandlingFilter> logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<NoelleExceptionHandlingFilter>>();
            logger.LogError(exception: context.Exception, message: context.Exception.Message);
        }

        context.HttpContext.Response.StatusCode = (int)statusCode;
        context.Result = new JsonResult(new NoelleErrorResponseDto(error));

        // 标记异常已被处理
        context.ExceptionHandled = true;

        return Task.CompletedTask;
    }
}
