using MediatR;
using Microsoft.EntityFrameworkCore;
using NoelleNet.EntityFrameworkCore;

namespace NoelleNet.AspNetCore.MediatR.Behaviors;

/// <summary>
/// 事务行为
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse?> where TRequest : notnull
{
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;
    private readonly IAppDbContext _dbContext;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="logger">日志</param>
    /// <param name="dbContext">数据库上下文</param>
    public TransactionBehavior(ILogger<TransactionBehavior<TRequest, TResponse>> logger, IAppDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    /// <summary>
    /// 管道行为处理
    /// </summary>
    /// <param name="request"></param>
    /// <param name="next"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<TResponse?> Handle(TRequest request, RequestHandlerDelegate<TResponse?> next, CancellationToken cancellationToken)
    {
        TResponse? response = default;
        string cmdName = request.GetGenericTypeName();

        try
        {
            // 如果已存在工作中的事务时，将不开启新的事务，且不对已存在的事务进行任何操作
            if (_dbContext.HasActiveTransaction)
                return await next();

            // 开启一个新的事务
            var strategy = _dbContext.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.BeginTransactionAsync();
                _logger.LogInformation("Begin transaction {TransactionId} for {CommandName} ({@Command})", transaction?.TransactionId, cmdName, request);

                response = await next();

                _logger.LogInformation("Commit transaction {TransactionId} for {CommandName} ({@Response})", transaction?.TransactionId, cmdName, response);
                await _dbContext.CommitTransactionAsync(transaction);

                _logger.LogInformation("Transaction finished {TransactionId} for {CommandName} ({@Response})", transaction?.TransactionId, cmdName, response);
            });

            return response;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "ERROR Handling transaction for {CommandName} ({@Command})", cmdName, request);
            throw;
        }
    }
}
