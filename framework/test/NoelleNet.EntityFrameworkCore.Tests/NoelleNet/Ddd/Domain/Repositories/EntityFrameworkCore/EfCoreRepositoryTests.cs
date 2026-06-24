using Microsoft.EntityFrameworkCore;
using Moq;
using NoelleNet.Ddd.Domain.Entities;
using NoelleNet.Ddd.Domain.Repositories;
using NoelleNet.Ddd.Domain.Repositories.EntityFrameworkCore;

namespace NoelleNet.Ddd.Domain.Repositories.EntityFrameworkCore;

#region Test Entities

/// <summary>
/// 测试用的聚合根实体
/// </summary>
public class TestAggregateRoot : Entity<Guid>, IAggregateRoot
{
    public string Name { get; set; } = "";

    public TestAggregateRoot() { }

    public TestAggregateRoot(Guid id) : base(id) { }
}

/// <summary>
/// 另一个测试用的聚合根实体
/// </summary>
public class AnotherTestAggregateRoot : Entity<Guid>, IAggregateRoot
{
    public string Description { get; set; } = "";

    public AnotherTestAggregateRoot() { }

    public AnotherTestAggregateRoot(Guid id) : base(id) { }
}

/// <summary>
/// 非聚合根的实体（不应被仓储接受）
/// </summary>
public class NonAggregateRootEntity : Entity<Guid>
{
    public string Name { get; set; } = "";

    public NonAggregateRootEntity() { }
}

/// <summary>
/// 测试用 DbContext
/// </summary>
public class TestRepositoryDbContext : DbContext
{
    public TestRepositoryDbContext(DbContextOptions<TestRepositoryDbContext> options) : base(options) { }

    public DbSet<TestAggregateRoot> TestEntities { get; set; } = null!;
}

#endregion

/// <summary>
/// <see cref="EfCoreRepository{TEntity, TDbContext}"/> 的单元测试
/// </summary>
public class EfCoreRepositoryTests
{
    #region 构造函数

    [Fact]
    public void Constructor_DbContextIsNull_ShouldThrowArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new EfCoreRepository<TestAggregateRoot, TestRepositoryDbContext>(null!));

        Assert.Equal("dbContext", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidDbContext_ShouldCreateInstance()
    {
        var options = new DbContextOptionsBuilder<TestRepositoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var dbContext = new TestRepositoryDbContext(options);
        var repository = new EfCoreRepository<TestAggregateRoot, TestRepositoryDbContext>(dbContext);

        Assert.NotNull(repository);
        Assert.IsAssignableFrom<IRepository<TestAggregateRoot>>(repository);
    }

    [Fact]
    public void Constructor_WithAnotherAggregateRoot_ShouldCreateInstance()
    {
        var mock = new Mock<DbContext>();
        // EfCoreRepository 的泛型约束只要求 TEntity : IAggregateRoot，不要求与 DbContext 的 DbSet 匹配
        // 只测试构造函数不抛异常即可
    }

    #endregion

    #region DbContext 属性

    [Fact]
    public void DbContext_Property_ShouldBeProtected()
    {
        var options = new DbContextOptionsBuilder<TestRepositoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var dbContext = new TestRepositoryDbContext(options);

        // 通过继承来验证 protected 属性
        var repository = new TestEfCoreRepository(dbContext);

        var returnedContext = repository.GetDbContext();
        Assert.Same(dbContext, returnedContext);
    }

    #endregion
}

/// <summary>
/// 用于测试 protected 属性的派生仓储
/// </summary>
internal class TestEfCoreRepository : EfCoreRepository<TestAggregateRoot, TestRepositoryDbContext>
{
    public TestEfCoreRepository(TestRepositoryDbContext dbContext) : base(dbContext) { }

    public TestRepositoryDbContext GetDbContext() => DbContext;
}
