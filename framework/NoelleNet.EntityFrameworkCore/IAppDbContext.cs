using Microsoft.EntityFrameworkCore.Storage;
using NoelleNet.Ddd.Domain.Entities;

namespace NoelleNet.EntityFrameworkCore;

public interface IAppDbContext : IUnitOfWork
{
    #region 事务
    /// <summary>
    /// 创建执行策略 <see cref="IExecutionStrategy"/>
    /// </summary>
    /// <returns></returns>
    IExecutionStrategy CreateExecutionStrategy();

    /// <summary>
    /// 当前事务的 <see cref="IDbContextTransaction"/> 对象
    /// </summary>
    IDbContextTransaction? CurrentTransaction { get; }

    /// <summary>
    /// 是否存在工作中的事务
    /// </summary>
    bool HasActiveTransaction { get; }

    /// <summary>
    /// 开启一个新的事务，如果当前存在已开启的事务，则返回null
    /// </summary>
    /// <returns><see cref="IDbContextTransaction"/> 事务实例</returns>
    Task<IDbContextTransaction?> BeginTransactionAsync();

    /// <summary>
    /// 提交事务
    /// </summary>
    /// <param name="transaction"><see cref="IDbContextTransaction"/> 对象，如果与 <see cref="CurrentTransaction"/> 不一致，则无法提交</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    Task CommitTransactionAsync(IDbContextTransaction? transaction);

    /// <summary>
    /// 事务回滚
    /// </summary>
    void RollbackTransaction();
    #endregion
}
