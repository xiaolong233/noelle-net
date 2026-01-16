using Microsoft.EntityFrameworkCore;
using NoelleNet.Ddd.Domain.Entities;

namespace NoelleNet.Ddd.Domain.Repositories.EntityFrameworkCore;

/// <summary>
/// 基于EfCore的 <see cref="INoelleRepository{T}"/> 实现
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
/// <typeparam name="TDbContext">数据库上下文类型</typeparam>
public class NoelleEfCoreRepository<TEntity, TDbContext> : INoelleRepository<TEntity> where TEntity : IAggregateRoot where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;

    /// <summary>
    /// 创建一个新的 <see cref="NoelleEfCoreRepository{TEntity, TDbContext}"/> 实例
    /// </summary>
    /// <param name="dbContext"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public NoelleEfCoreRepository(TDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// 数据库上下文
    /// </summary>
    protected TDbContext DbContext => _dbContext;
}
