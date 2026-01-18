using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using NoelleNet.Http;
using NoelleNet.Logging;
using System.Net;
using System.Text;
using System.Text.Json;

namespace NoelleNet.AspNetCore.ExceptionHandling;

/// <summary>
/// 全局异常处理筛选器
/// </summary>
public class NoelleExceptionHandlingFilter : IAsyncExceptionFilter
{
    private readonly IExceptionToErrorConverter _converter;
    private readonly IHttpExceptionStatusCodeFinder _finder;
    private readonly IOptions<NoelleExceptionHandlingOptions> _exceptionHandlingOptions;
    private readonly JsonSerializerOptions _serializerOptions;

    /// <summary>
    /// 创建一个新的 <see cref="NoelleExceptionHandlingFilter"/> 实例
    /// </summary>
    /// <param name="converter">异常信息转换器</param>
    /// <param name="finder">HTTP错误状态码查找器</param>
    /// <param name="exceptionHandlingOptions">异常处理选项</param>
    /// <exception cref="ArgumentNullException"></exception>
    public NoelleExceptionHandlingFilter(IExceptionToErrorConverter converter, IHttpExceptionStatusCodeFinder finder, IOptions<NoelleExceptionHandlingOptions> exceptionHandlingOptions)
    {
        _converter = converter ?? throw new ArgumentNullException(nameof(converter));
        _finder = finder ?? throw new ArgumentNullException(nameof(finder));
        _exceptionHandlingOptions = exceptionHandlingOptions ?? throw new ArgumentNullException(nameof(exceptionHandlingOptions));

        _serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.General);
        _serializerOptions.WriteIndented = true; // 格式化输出
    }

    /// <summary>
    /// 在操作引发 <see cref="Exception"/> 后调用 
    /// </summary>
    /// <param name="context">异常上下文</param>
    /// <returns></returns>
    public Task OnExceptionAsync(ExceptionContext context)
    {
        if (context.ExceptionHandled)
            return Task.CompletedTask;

        // 把异常对象转换成错误信息对象
        var error = _converter.Covert(context.Exception, options =>
        {
            options.TraceIdProvider = _exceptionHandlingOptions.Value.TraceIdProvider ?? (() => context.HttpContext.TraceIdentifier);
            options.IncludeExceptionDetails = _exceptionHandlingOptions.Value.IncludeExceptionDetails;
            options.IncludeStackTrace = _exceptionHandlingOptions.Value.IncludeStackTrace;
            options.IncludeExceptionData = _exceptionHandlingOptions.Value.IncludeExceptionData;
        });

        // 根据异常获取对应的HTTP状态码
        HttpStatusCode statusCode = _finder.GetStatusCode(context.HttpContext, context.Exception);

        // 记录日志信息
        ILogger<NoelleExceptionHandlingFilter> logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<NoelleExceptionHandlingFilter>>();
        var logLevel = LogLevel.Error;
        if (context.Exception is IHasLogLevel hasLogLevel)
        {
            logLevel = hasLogLevel.LogLevel;
        }
        else if (context.Exception is IBusinessException)
        {
            logLevel = LogLevel.Information;
        }

        StringBuilder messageBuilder = new StringBuilder();
        messageBuilder.AppendLine($"============ {nameof(RemoteCallErrorInfo)} ============");
        messageBuilder.AppendLine(JsonSerializer.Serialize(error, _serializerOptions));
        logger.Log(logLevel, context.Exception, message: messageBuilder.ToString(), []);

        context.HttpContext.Response.StatusCode = (int)statusCode;
        context.Result = new JsonResult(new ErrorResponseDto(error));

        // 标记异常已被处理
        context.ExceptionHandled = true;

        return Task.CompletedTask;
    }
}
