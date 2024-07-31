using Microsoft.EntityFrameworkCore;
using NoelleNet.Ddd.Domain.Entities;

namespace NoelleNet.EntityFrameworkCore;

/// <summary>
/// EfCore的仓储基类
/// </summary>
/// <typeparam name="TDbContext">派生自 <see cref="AppDbContext"/> 的数据库上下文类型</typeparam>
/// <typeparam name="TAggregateRoot">聚合根实体类型</typeparam>
public class EfCoreRepository<TDbContext, TAggregateRoot> : IRepository<TAggregateRoot> where TDbContext : DbContext, IUnitOfWork where TAggregateRoot : IAggregateRoot
{
    private readonly TDbContext _dbContext;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="context">数据库上下文对象</param>
    /// <exception cref="ArgumentNullException"></exception>
    public EfCoreRepository(TDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    /// <summary>
    /// 数据库上下文对象
    /// </summary>
    protected TDbContext DbContext => _dbContext;

    /// <summary>
    /// 工作单元
    /// </summary>
    public virtual IUnitOfWork UnitOfWork => _dbContext;
}
