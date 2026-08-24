using Microsoft.AspNetCore.Diagnostics;
using NoelleNet.Logging;
using System.Diagnostics;
using System.Net;

namespace NoelleNet.AspNetCore.ExceptionHandling;

/// <summary>
/// 全局异常处理程序
/// </summary>
public class NoelleExceptionHandler : IExceptionHandler
{
    private readonly ILogger<NoelleExceptionHandler> _logger;
    private readonly IErrorResponseWriter _writer;

    /// <summary>
    /// 创建一个新的 <see cref="NoelleExceptionHandler"/> 实例
    /// </summary>
    /// <param name="logger"><see cref="ILogger{TCategoryName}"/> 实例</param>
    /// <param name="writer"><see cref="IErrorResponseWriter"/> 实例</param>
    /// <exception cref="ArgumentNullException"></exception>
    public NoelleExceptionHandler(ILogger<NoelleExceptionHandler> logger, IErrorResponseWriter writer)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }

    /// <inheritdoc/>
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        try
        {
            // 写入错误响应内容
            return await _writer.TryWriteAsync(httpContext, exception, cancellationToken);
        }
        catch
        {
            // 写入失败时不向上抛出，返回 false 交由其他异常处理器或默认行为处理
            return false;
        }
        finally
        {
            // 记录日志，优先记录实际响应的状态码，若状态码未被设置（仍为默认值且响应未开始）则按服务器内部错误记录
            LogLevel logLevel = exception switch
            {
                IHasLogLevel hasLogLevel => hasLogLevel.LogLevel,
                IBusinessException => LogLevel.Information,
                TaskCanceledException => LogLevel.Debug,
                OperationCanceledException => LogLevel.Warning,
                _ => LogLevel.Error
            };
            int statusCode = httpContext.Response.StatusCode;
            if (statusCode == (int)HttpStatusCode.OK && !httpContext.Response.HasStarted)
                statusCode = (int)HttpStatusCode.InternalServerError;

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
