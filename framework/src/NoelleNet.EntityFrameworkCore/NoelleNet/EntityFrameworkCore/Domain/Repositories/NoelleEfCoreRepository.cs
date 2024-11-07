using Microsoft.EntityFrameworkCore;
using NoelleNet.Ddd.Domain.Entities;

namespace NoelleNet.Ddd.Domain.Repositories.EntityFrameworkCore;

/// <summary>
/// 基于EfCore的 <see cref="INoelleRepository{T}"/> 实现
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
/// <typeparam name="TDbContext">数据库上下文类型</typeparam>
/// <param name="dbContext">数据库上下文</param>
public class NoelleEfCoreRepository<TEntity, TDbContext>(TDbContext dbContext) : INoelleRepository<TEntity> where TEntity : IAggregateRoot where TDbContext : DbContext
{
    private readonly TDbContext _dbContext = dbContext;

    /// <summary>
    /// 数据库上下文实例
    /// </summary>
    protected TDbContext DbContext => _dbContext;

    public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
