using Microsoft.EntityFrameworkCore;

namespace NoelleNet.Uow;

/// <summary>
/// 基于 EF Core 的 <see cref="IUnitOfWork"/> 默认实现
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _dbContext;

    /// <summary>
    /// 创建一个新的 <see cref="UnitOfWork"/> 实例
    /// </summary>
    /// <param name="dbContext">数据库上下文</param>
    /// <exception cref="ArgumentNullException"></exception>
    public UnitOfWork(DbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <inheritdoc/>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
