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
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITransactionManager _transactionManager;

    /// <summary>
    /// 创建一个新的 <see cref="NoelleTransactionBehavior{TRequest, TResponse}"/> 实例
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="dbContext">数据库上下文</param>
    /// <param name="unitOfWork">工作单元</param>
    /// <param name="transactionManager">事务管理器</param>
    /// <exception cref="ArgumentNullException"></exception>
    public NoelleTransactionBehavior(ILogger<NoelleTransactionBehavior<TRequest, TResponse>> logger, DbContext dbContext, IUnitOfWork unitOfWork, ITransactionManager transactionManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _transactionManager = transactionManager ?? throw new ArgumentNullException(nameof(transactionManager));
    }

    /// <inheritdoc />
    public async Task<TResponse?> Handle(TRequest request, RequestHandlerDelegate<TResponse?> next, CancellationToken cancellationToken)
    {
        string cmdName = request.GetGenericTypeName();

        if (_transactionManager.HasActiveTransaction)
        {
            return await next(cancellationToken);
        }

        TResponse? response = default;
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            try
            {
                await _transactionManager.BeginAsync(cancellationToken);
                var transactionId = _transactionManager.TransactionId;
                _logger.LogInformation("开始事务 {TransactionId} - 命令: {CommandName} ({@Command})", transactionId, cmdName, request);

                response = await next(cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _transactionManager.CommitAsync(cancellationToken);

                _logger.LogInformation("事务完成 {TransactionId} - 命令: {CommandName} ({@Response})", transactionId, cmdName, response);
            }
            catch (Exception e)
            {
                if (_transactionManager.HasActiveTransaction)
                {
                    await _transactionManager.RollbackAsync(cancellationToken);
                }

                _logger.LogError(e, "事务处理错误 - 命令: {CommandName} ({@Command})", cmdName, request);
                throw;
            }
        });

        return response;
    }
}
