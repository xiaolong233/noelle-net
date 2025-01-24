using DotNetCore.CAP;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace NoelleNet.EntityFrameworkCore.Storage;

/// <summary>
/// 基于 <see cref="ICapTransaction"/> 的数据库事务管道
/// </summary>
/// <remarks>CAP在发布消息时需要通过 <see cref="ICapTransaction"/> 的实例来提交事务</remarks>
/// <typeparam name="TRequest">请求类型</typeparam>
/// <typeparam name="TResponse">响应类型</typeparam>
/// <param name="logger">日志记录器</param>
/// <param name="dbContext">数据库上下文实例</param>
/// <param name="capPublisher">分布式事件总线</param>
public class NoelleCapTransactionBehavior<TRequest, TResponse>(
    ILogger<NoelleCapTransactionBehavior<TRequest, TResponse>> logger,
    DbContext dbContext,
    ICapPublisher capPublisher
    ) : IPipelineBehavior<TRequest, TResponse?> where TRequest : notnull
{
    private readonly ILogger<NoelleCapTransactionBehavior<TRequest, TResponse>> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly DbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly ICapPublisher _capPublisher = capPublisher ?? throw new ArgumentNullException(nameof(capPublisher));

    /// <summary>
    /// 管道处理函数
    /// </summary>
    /// <param name="request">请求对象</param>
    /// <param name="next">请求处理委托</param>
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
            _logger.LogInformation("开始事务 {TransactionId} - 命令: {CommandName} ({@Command})", transaction.TransactionId, cmdName, request);

            try
            {
                response = await next();

                _logger.LogInformation("提交事务 {TransactionId} - 命令: {CommandName} ({@Response})", transaction.TransactionId, cmdName, response);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("事务完成 {TransactionId} - 命令: {CommandName} ({@Response})", transaction.TransactionId, cmdName, response);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync(cancellationToken);

                _logger.LogError(e, "事务处理错误 - 命令: {CommandName} ({@Command})", cmdName, request);
                throw;
            }

        });

        return response;
    }
}
