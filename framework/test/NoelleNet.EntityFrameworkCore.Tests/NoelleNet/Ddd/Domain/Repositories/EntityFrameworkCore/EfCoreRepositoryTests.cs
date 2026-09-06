using Microsoft.EntityFrameworkCore;
using NoelleNet.Ddd.Domain.Entities;
using NoelleNet.Ddd.Domain.Repositories;
using NoelleNet.Ddd.Domain.Repositories.EntityFrameworkCore;

namespace NoelleNet.Ddd.Domain.Repositories.EntityFrameworkCore;

/// <summary>
/// <see cref="EfCoreRepository{TEntity, TDbContext}"/> 的契约测试
/// </summary>
public class EfCoreRepositoryTests
{
    /// <summary>
    /// protected DbContext 属性应暴露注入的上下文实例（派生仓储实现查询的唯一入口）
    /// </summary>
    [Fact]
    public void DbContext_Property_ShouldExposeInjectedContext()
    {
        var options = new DbContextOptionsBuilder<TestRepositoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var dbContext = new TestRepositoryDbContext(options);

        var repository = new TestEfCoreRepository(dbContext);

        Assert.Same(dbContext, repository.GetDbContext());
    }
}

/// <summary>
/// 测试用的聚合根实体
/// </summary>
public class TestAggregateRoot : Entity<Guid>, IAggregateRoot
{
    public string Name { get; set; } = "";
}

/// <summary>
/// 测试用 DbContext
/// </summary>
public class TestRepositoryDbContext : DbContext
{
    public TestRepositoryDbContext(DbContextOptions<TestRepositoryDbContext> options) : base(options) { }

    public DbSet<TestAggregateRoot> TestEntities { get; set; } = null!;
}

/// <summary>
/// 暴露 protected DbContext 属性的派生仓储
/// </summary>
internal class TestEfCoreRepository : EfCoreRepository<TestAggregateRoot, TestRepositoryDbContext>
{
    public TestEfCoreRepository(TestRepositoryDbContext dbContext) : base(dbContext) { }

    public TestRepositoryDbContext GetDbContext() => DbContext;
}
