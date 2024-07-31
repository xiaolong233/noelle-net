using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using NoelleNet.Ddd.Domain.Entities;
using System.Data;

namespace NoelleNet.EntityFrameworkCore;

/// <summary>
/// 应用的数据库上下文
/// </summary>
public class AppDbContext : DbContext, IAppDbContext
{
    private readonly IMediator _mediator;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="mediator"></param>
    public AppDbContext(DbContextOptions options, IMediator mediator) : base(options)
    {
        _mediator = mediator;
    }

    #region 事务
    public IExecutionStrategy CreateExecutionStrategy()
    {
        return Database.CreateExecutionStrategy();
    }

    private IDbContextTransaction? _currentTransaction;
    /// <summary>
    /// 当前事务的 <see cref="IDbContextTransaction"/> 对象
    /// </summary>
    public IDbContextTransaction? CurrentTransaction
    {
        get { return _currentTransaction; }
    }

    /// <summary>
    /// 是否存在工作中的事务
    /// </summary>
    public bool HasActiveTransaction => _currentTransaction != null;

    /// <summary>
    /// 开启一个新的事务
    /// </summary>
    /// <returns><see cref="IDbContextTransaction"/> 事务实例</returns>
    public virtual async Task<IDbContextTransaction?> BeginTransactionAsync()
    {
        if (_currentTransaction != null)
            return null;

        _currentTransaction = await Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        return _currentTransaction;
    }

    /// <summary>
    /// 提交修改和事务
    /// </summary>
    /// <param name="transaction"><see cref="IDbContextTransaction"/> 对象，如果与 <see cref="CurrentTransaction"/> 不一致，则无法提交</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task CommitTransactionAsync(IDbContextTransaction? transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        if (transaction != _currentTransaction)
            throw new InvalidOperationException($"事务[{transaction.TransactionId}]与当前最新开启的事务不一致");

        try
        {
            await SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            RollbackTransaction();
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    /// <summary>
    /// 事务回滚
    /// </summary>
    public void RollbackTransaction()
    {
        try
        {
            _currentTransaction?.Rollback();
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }
    #endregion

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 触发领域事件的调度
        // 在同一个请求中，领域事件处理程序中使用的是同一个DbContext实例
        await DispatchDomainEventsAsync(cancellationToken);

        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 派发领域事件
    /// </summary>
    /// <param name="cancellationToken"></param>
    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken = default)
    {
        // 获取所有发生更改，且含有领域事件的实体对象
        var entities = ChangeTracker.Entries<IHasDomainEvents>()
                                    .Where(e => e.Entity.DomainEvents != null && e.Entity.DomainEvents.Count > 0)
                                    .ToList();

        // 获取所有领域事件
        var domainEvents = entities.SelectMany(e => e.Entity.DomainEvents).ToList();

        // 清空实体里的领域事件
        entities.ForEach(e => e.Entity.ClearDomainEvents());

        // 发布领域事件
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }
}
