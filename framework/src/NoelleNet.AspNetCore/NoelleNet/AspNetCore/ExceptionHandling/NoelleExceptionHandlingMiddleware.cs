using NoelleNet.Logging;
using NoelleNet.Validation;
using System.Data;
using System.Diagnostics;
using System.Net;

namespace NoelleNet.AspNetCore.ExceptionHandling;

/// <summary>
/// 全局异常处理中间件
/// </summary>
public class NoelleExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<NoelleExceptionHandlingMiddleware> _logger;
    private readonly IErrorResponseWriter _errorResponseWriter;

    /// <summary>
    /// 创建一个新的 <see cref="NoelleExceptionHandlingMiddleware"/> 实例
    /// </summary>
    /// <param name="next">请求管道中的下一个中间件</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="errorResponseWriter">错误响应写入器</param>
    /// <exception cref="ArgumentNullException"></exception>
    public NoelleExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<NoelleExceptionHandlingMiddleware> logger,
        IErrorResponseWriter errorResponseWriter)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _errorResponseWriter = errorResponseWriter ?? throw new ArgumentNullException(nameof(errorResponseWriter));
    }

    /// <summary>
    /// 处理 HTTP 请求
    /// </summary>
    /// <param name="context">当前请求的 <see cref="HttpContext"/> 实例</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception e)
        {
            await HandleExceptionAsync(context, e);
        }
    }

    /// <summary>
    /// 处理请求过程中发生的异常
    /// </summary>
    /// <param name="httpContext">当前请求的 <see cref="HttpContext"/> 实例</param>
    /// <param name="exception">请求过程中捕获的异常</param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    protected virtual async Task HandleExceptionAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken = default)
    {
        try
        {
            // 尝试将错误信息写入 HTTP 响应
            await _errorResponseWriter.TryWriteAsync(httpContext, exception, cancellationToken);
        }
        finally
        {
            int statusCode = httpContext.Response.StatusCode;
            if (statusCode == (int)HttpStatusCode.OK && !httpContext.Response.HasStarted)
                statusCode = (int)HttpStatusCode.InternalServerError;

            LogLevel logLevel = GetLogLevel(exception);

            if (_logger.IsEnabled(logLevel))
            {
                _logger.Log(
                    logLevel,
                    exception,
                    "处理请求时发生异常。TraceId：{TraceId}，请求方法：{RequestMethod}，请求路径：{RequestPath}，响应状态码：{StatusCode}，异常类型：{ExceptionType}",
                    Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier,
                    httpContext.Request.Method,
                    httpContext.Request.Path,
                    statusCode,
                    exception.GetType().FullName);
            }
        }
    }

    protected virtual LogLevel GetLogLevel(Exception e) => e switch
    {
        IHasLogLevel hasLogLevel => hasLogLevel.LogLevel,
        IHasValidationResults => LogLevel.Information,
        System.ComponentModel.DataAnnotations.ValidationException => LogLevel.Information,
        IBusinessException => LogLevel.Information,
        DBConcurrencyException => LogLevel.Warning,
        OperationCanceledException { InnerException: TimeoutException } => LogLevel.Warning,
        OperationCanceledException => LogLevel.Debug,
        _ => LogLevel.Error
    };
}
