using DotNetCore.CAP;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace NoelleNet.EntityFrameworkCore.Storage;

/// <summary>
/// 基于 <see cref="ICapTransaction"/> 的数据库事务管道
/// </summary>
/// <remarks>CAP在发布消息时需要通过 <see cref="ICapTransaction"/> 的实例来提交事务</remarks>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
/// <param name="logger">日志组件</param>
/// <param name="dbContext">数据库上下文实例</param>
/// <param name="capPublisher">分布式事件总线实例</param>
public class NoelleTransactionBehavior<TRequest, TResponse>(ILogger<NoelleTransactionBehavior<TRequest, TResponse>> logger, DbContext dbContext, ICapPublisher capPublisher) : IPipelineBehavior<TRequest, TResponse?> where TRequest : notnull
{
    private readonly ILogger<NoelleTransactionBehavior<TRequest, TResponse>> _logger = logger;
    private readonly DbContext _dbContext = dbContext;
    private readonly ICapPublisher _capPublisher = capPublisher;

    /// <summary>
    /// 管道处理函数
    /// </summary>
    /// <param name="request"></param>
    /// <param name="next"></param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<TResponse?> Handle(TRequest request, RequestHandlerDelegate<TResponse?> next, CancellationToken cancellationToken)
    {
        TResponse? response = default;
        string cmdName = request.GetGenericTypeName();

        // 开启一个新的事务
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(_capPublisher, false, cancellationToken) ?? throw new InvalidOperationException("数据库事务开启失败");
            _logger.LogInformation("Begin transaction {TransactionId} for {CommandName} ({@Command})", transaction?.TransactionId, cmdName, request);

            try
            {
                response = await next();

                _logger.LogInformation("Commit transaction {TransactionId} for {CommandName} ({@Response})", transaction?.TransactionId, cmdName, response);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction!.CommitAsync();

                _logger.LogInformation("Transaction finished {TransactionId} for {CommandName} ({@Response})", transaction?.TransactionId, cmdName, response);
            }
            catch (Exception e)
            {
                await transaction!.RollbackAsync(cancellationToken);

                _logger.LogError(e, "ERROR Handling transaction for {CommandName} ({@Command})", cmdName, request);
                throw;
            }

        });

        return response;
    }
}
