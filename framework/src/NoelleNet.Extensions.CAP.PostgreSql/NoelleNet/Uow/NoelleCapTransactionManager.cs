using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace NoelleNet.Uow;

/// <summary>
/// 支持 CAP 事务发布消息的 <see cref="ITransactionManager"/> 实现
/// </summary>
public class NoelleCapTransactionManager : NoelleTransactionManager
{
    private readonly DbContext _dbContext;
    private readonly ICapPublisher _capPublisher;

    /// <summary>
    /// 创建一个新的 <see cref="NoelleCapTransactionManager"/> 实例
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    /// <param name="capPublisher"><see cref="ICapPublisher"/> 实例</param>
    /// <exception cref="ArgumentNullException"></exception>
    public NoelleCapTransactionManager(DbContext dbContext, ICapPublisher capPublisher) : base(dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _capPublisher = capPublisher ?? throw new ArgumentNullException(nameof(capPublisher));
    }

    /// <inheritdoc/>
    public override Task BeginAsync(CancellationToken cancellationToken = default)
    {
        if (HasActiveTransaction)
            throw new InvalidOperationException("当前已经存在一个活动的事务。请确保之前的事务已提交或回滚后，再开始新的事务。");

        // 使用同步方法开启事务，避免 CAP 的 AsyncLocal 在异步上下文中失效
        CurrentTransaction = _dbContext.Database.BeginTransaction(_capPublisher, false);

        return Task.FromResult(CurrentTransaction);
    }

    /// <inheritdoc/>
    public override Task BeginAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        if (HasActiveTransaction)
            throw new InvalidOperationException("当前已经存在一个活动的事务。请确保之前的事务已提交或回滚后，再开始新的事务。");

        // 使用同步方法开启事务，避免 CAP 的 AsyncLocal 在异步上下文中失效
        CurrentTransaction = _dbContext.Database.BeginTransaction(isolationLevel, _capPublisher, false);

        return Task.FromResult(CurrentTransaction);
    }
}
