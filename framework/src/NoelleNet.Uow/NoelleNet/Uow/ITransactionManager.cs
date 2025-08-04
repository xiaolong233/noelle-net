namespace NoelleNet.Uow;

/// <summary>
/// 事务管理器接口
/// </summary>
public interface ITransactionManager : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// 是否存在已激活的事务
    /// </summary>
    bool HasActiveTransaction { get; }

    /// <summary>
    /// 事务标识
    /// </summary>
    Guid? TransactionId { get; }

    /// <summary>
    /// 启用一个新的事务
    /// </summary>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    Task BeginAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 提交对当前事务中数据库所做的所有更改
    /// </summary>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 放弃对当前事务中数据库所做的所有更改
    /// </summary>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
