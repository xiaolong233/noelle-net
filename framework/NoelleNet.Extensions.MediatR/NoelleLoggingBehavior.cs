using MediatR;
using Microsoft.Extensions.Logging;

namespace NoelleNet.Extensions.MediatR;

/// <summary>
/// MediatR行为管道的日志记录器
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
/// <param name="logger"></param>
public class NoelleLoggingBehavior<TRequest, TResponse>(ILogger<NoelleLoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ILogger<NoelleLoggingBehavior<TRequest, TResponse>> _logger = logger;

    /// <summary>
    /// 管道行为处理
    /// </summary>
    /// <param name="request"></param>
    /// <param name="next"></param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Command starting {CommandName} ({@Command})", request.GetGenericTypeName(), request);
        var response = await next();
        _logger.LogInformation("Command finished {CommandName} ({@Response})", request.GetGenericTypeName(), response);

        return response;
    }
}
