using MediatR;
using Microsoft.Extensions.Logging;

namespace NoelleNet.Extensions.MediatR;

/// <summary>
/// MediatR行为管道的日志记录器
/// </summary>
/// <typeparam name="TRequest">请求类型</typeparam>
/// <typeparam name="TResponse">响应类型</typeparam>
/// <param name="logger">日志记录器</param>
public class NoelleLoggingBehavior<TRequest, TResponse>(
    ILogger<NoelleLoggingBehavior<TRequest, TResponse>> logger
    ) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly ILogger<NoelleLoggingBehavior<TRequest, TResponse>> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// 管道行为处理
    /// </summary>
    /// <param name="request">请求对象</param>
    /// <param name="next">请求处理委托</param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("命令开始 {CommandName} ({@Command})", request.GetGenericTypeName(), request);
        var response = await next();
        _logger.LogInformation("命令完成 {CommandName} ({@Response})", request.GetGenericTypeName(), response);

        return response;
    }
}
