using Microsoft.EntityFrameworkCore;
using NoelleNet.Ddd.Domain.Repositories;

namespace NoelleNet.EntityFrameworkCore.Domain.Repositories;

/// <summary>
/// 工作单元装饰器
/// </summary>
/// <param name="dbContext">数据库上下文</param>
public class NoelleUnitOfWorkDecorator(DbContext dbContext) : IUnitOfWork
{
    private readonly DbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
