using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NoelleNet.Uow;

namespace NoelleNet.Extensions.MediatR;

/// <summary>
/// 事务处理管道
/// </summary>
/// <typeparam name="TRequest">请求类型</typeparam>
/// <typeparam name="TResponse">响应类型</typeparam>
public class NoelleTransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse?> where TRequest : notnull
{
    private readonly ILogger<NoelleTransactionBehavior<TRequest, TResponse>> _logger;
    private readonly DbContext _dbContext;
    private readonly ITransactionManager _transactionManager;
    
    /// <summary>
    /// 创建一个新的 <see cref="NoelleTransactionBehavior{TRequest, TResponse}"/> 实例
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="dbContext">数据库上下文</param>
    /// <param name="transactionManager">事务管理器</param>
    /// <exception cref="ArgumentNullException"></exception>
    public NoelleTransactionBehavior(ILogger<NoelleTransactionBehavior<TRequest, TResponse>> logger, DbContext dbContext, ITransactionManager transactionManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _transactionManager = transactionManager ?? throw new ArgumentNullException(nameof(transactionManager));
    }

    /// <summary>
    /// 管道行为处理
    /// </summary>
    /// <param name="request">请求对象</param>
    /// <param name="next">请求处理委托</param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    public async Task<TResponse?> Handle(TRequest request, RequestHandlerDelegate<TResponse?> next, CancellationToken cancellationToken)
    {
        TResponse? response = default;
        string cmdName = request.GetGenericTypeName();

        if (_transactionManager.HasActiveTransaction)
        {
            return await next(cancellationToken);
        }

        // 开启一个新的事务
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await _transactionManager.BeginAsync(cancellationToken);
            _logger.LogInformation("开始事务 {TransactionId} - 命令: {CommandName} ({@Command})", _transactionManager.TransactionId, cmdName, request);

            try
            {
                response = await next();

                _logger.LogInformation("提交事务 {TransactionId} - 命令: {CommandName} ({@Response})", _transactionManager.TransactionId, cmdName, response);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await _transactionManager.CommitAsync();

                _logger.LogInformation("事务完成 {TransactionId} - 命令: {CommandName} ({@Response})", _transactionManager.TransactionId, cmdName, response);
            }
            catch (Exception e)
            {
                await _transactionManager.RollbackAsync(cancellationToken);

                _logger.LogError(e, "事务处理错误 - 命令: {CommandName} ({@Command})", cmdName, request);
                throw;
            }

        });

        return response;
    }
}
