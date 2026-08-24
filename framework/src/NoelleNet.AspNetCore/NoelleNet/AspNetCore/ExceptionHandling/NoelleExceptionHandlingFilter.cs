using Microsoft.AspNetCore.Mvc.Filters;
using NoelleNet.Logging;
using System.Diagnostics;
using System.Net;

namespace NoelleNet.AspNetCore.ExceptionHandling;

/// <summary>
/// 全局异常处理筛选器
/// </summary>
public class NoelleExceptionHandlingFilter : IAsyncExceptionFilter
{
    private readonly ILogger<NoelleExceptionHandlingFilter> _logger;
    private readonly IErrorResponseWriter _errorResponseWriter;

    /// <summary>
    /// 创建一个新的 <see cref="NoelleExceptionHandlingFilter"/> 实例
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="errorResponseWriter"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public NoelleExceptionHandlingFilter(
        ILogger<NoelleExceptionHandlingFilter> logger,
        IErrorResponseWriter errorResponseWriter)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _errorResponseWriter = errorResponseWriter ?? throw new ArgumentNullException(nameof(errorResponseWriter));
    }

    /// <inheritdoc/>
    public async Task OnExceptionAsync(ExceptionContext context)
    {
        if (context.ExceptionHandled)
            return;

        try
        {
            // 尝试将错误信息写入 HTTP 响应，写入失败时交由 MVC 默认处理
            context.ExceptionHandled = await _errorResponseWriter.TryWriteAsync(context.HttpContext, context.Exception);
        }
        finally
        {
            // 记录日志，优先记录实际响应的状态码，若状态码未被设置（仍为默认值且响应未开始）则按服务器内部错误记录
            LogLevel logLevel = context.Exception switch
            {
                IHasLogLevel hasLogLevel => hasLogLevel.LogLevel,
                IBusinessException => LogLevel.Information,
                TaskCanceledException => LogLevel.Debug,
                OperationCanceledException => LogLevel.Warning,
                _ => LogLevel.Error
            };
            int statusCode = context.HttpContext.Response.StatusCode;
            if (statusCode == (int)HttpStatusCode.OK && !context.HttpContext.Response.HasStarted)
                statusCode = (int)HttpStatusCode.InternalServerError;

            _logger.Log(
                logLevel,
                context.Exception,
                "处理请求时发生异常。TraceId：{TraceId}，请求方法：{RequestMethod}，请求路径：{RequestPath}，响应状态码：{StatusCode}，异常类型：{ExceptionType}",
                Activity.Current?.TraceId.ToString() ?? context.HttpContext.TraceIdentifier,
                context.HttpContext.Request.Method,
                context.HttpContext.Request.Path,
                statusCode,
                context.Exception.GetType().FullName);
        }
    }
}
