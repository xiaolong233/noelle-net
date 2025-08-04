using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace NoelleNet.Uow;

/// <summary>
/// 基于 EF Core 的 <see cref="ITransactionManager"/> 默认实现
/// </summary>
public class NoelleTransactionManager : ITransactionManager
{
    private readonly DbContext _dbContext;

    /// <summary>
    /// 创建一个新的 <see cref="NoelleTransactionManager"/> 实例
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    /// <exception cref="ArgumentNullException"></exception>
    public NoelleTransactionManager(DbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// 当前已激活的事务
    /// </summary>
    protected IDbContextTransaction? CurrentTransaction { get; set; }

    /// <inheritdoc/>
    public virtual bool HasActiveTransaction => CurrentTransaction != null;

    /// <inheritdoc/>
    public virtual Guid? TransactionId => CurrentTransaction?.TransactionId;

    /// <inheritdoc/>
    public virtual async Task BeginAsync(CancellationToken cancellationToken = default)
    {
        if (HasActiveTransaction)
            throw new InvalidOperationException("当前已经存在一个活动的事务。请确保之前的事务已提交或回滚后，再开始新的事务。");

        CurrentTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task BeginAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        if (HasActiveTransaction)
            throw new InvalidOperationException("当前已经存在一个活动的事务。请确保之前的事务已提交或回滚后，再开始新的事务。");

        CurrentTransaction = await _dbContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentTransaction == null)
            throw new InvalidOperationException("没有活动事务可提交。请确保在提交之前已经开始一个有效的事务。");

        await CurrentTransaction.CommitAsync(cancellationToken);
        await CurrentTransaction.DisposeAsync();

        CurrentTransaction = null;
    }

    /// <inheritdoc/>
    public virtual async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentTransaction == null)
            throw new InvalidOperationException("没有活动事务可回滚。请确保在回滚之前已经开始一个有效的事务。");

        await CurrentTransaction.RollbackAsync(cancellationToken);
        await CurrentTransaction.DisposeAsync();

        CurrentTransaction = null;
    }

    /// <inheritdoc/>
    public virtual void Dispose()
    {
        if (CurrentTransaction == null)
            return;

        CurrentTransaction.Dispose();
        CurrentTransaction = null;

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public virtual async ValueTask DisposeAsync()
    {
        if (CurrentTransaction == null)
            return;

        await CurrentTransaction.DisposeAsync();
        CurrentTransaction = null;

        GC.SuppressFinalize(this);
    }
}
