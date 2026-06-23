using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using NoelleNet.Http;
using NoelleNet.Logging;
using System.Net;

namespace NoelleNet.AspNetCore.ExceptionHandling;

/// <summary>
/// 全局异常处理筛选器
/// </summary>
public class NoelleExceptionHandlingFilter : IAsyncExceptionFilter
{
    private readonly ILogger<NoelleExceptionHandlingFilter> _logger;
    private readonly IExceptionToErrorConverter _converter;
    private readonly IHttpExceptionStatusCodeFinder _finder;
    private readonly IOptions<NoelleExceptionHandlingOptions> _exceptionHandlingOptions;

    /// <summary>
    /// 创建一个新的 <see cref="NoelleExceptionHandlingFilter"/> 实例
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="converter">异常信息转换器</param>
    /// <param name="finder">HTTP错误状态码查找器</param>
    /// <param name="exceptionHandlingOptions">异常处理选项</param>
    /// <exception cref="ArgumentNullException"></exception>
    public NoelleExceptionHandlingFilter(ILogger<NoelleExceptionHandlingFilter> logger, IExceptionToErrorConverter converter, IHttpExceptionStatusCodeFinder finder, IOptions<NoelleExceptionHandlingOptions> exceptionHandlingOptions)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _converter = converter ?? throw new ArgumentNullException(nameof(converter));
        _finder = finder ?? throw new ArgumentNullException(nameof(finder));
        _exceptionHandlingOptions = exceptionHandlingOptions ?? throw new ArgumentNullException(nameof(exceptionHandlingOptions));
    }

    /// <inheritdoc/>
    public Task OnExceptionAsync(ExceptionContext context)
    {
        if (context.ExceptionHandled)
            return Task.CompletedTask;

        var error = _converter.Covert(context.Exception, options =>
        {
            options.TraceIdProvider = _exceptionHandlingOptions.Value.TraceIdProvider ?? (() => context.HttpContext.TraceIdentifier);
            options.IncludeExceptionDetails = _exceptionHandlingOptions.Value.IncludeExceptionDetails;
            options.IncludeStackTrace = _exceptionHandlingOptions.Value.IncludeStackTrace;
            options.IncludeExceptionData = _exceptionHandlingOptions.Value.IncludeExceptionData;
        });

        HttpStatusCode statusCode = _finder.GetStatusCode(context.HttpContext, context.Exception);

        LogLevel logLevel = context.Exception switch
        {
            IHasLogLevel hasLogLevel => hasLogLevel.LogLevel,
            IBusinessException => LogLevel.Information,
            _ => LogLevel.Error
        };

        _logger.Log(logLevel, context.Exception, "{@ErrorInfo}", error);

        context.Result = new JsonResult(new ErrorResponseDto(error))
        {
            StatusCode = (int)statusCode
        };

        context.ExceptionHandled = true;

        return Task.CompletedTask;
    }
}
