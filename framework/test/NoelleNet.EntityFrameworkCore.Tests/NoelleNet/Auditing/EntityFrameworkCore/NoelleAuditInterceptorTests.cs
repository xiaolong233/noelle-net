using Microsoft.EntityFrameworkCore;
using Moq;
using NoelleNet.Auditing;
using NoelleNet.Security;

namespace NoelleNet.Auditing.EntityFrameworkCore;

/// <summary>
/// <see cref="NoelleAuditInterceptor"/> 的行为测试：创建/修改审计字段的自动填充
/// </summary>
public class NoelleAuditInterceptorTests
{
    /// <summary>
    /// 新增审计实体应填充 CreatedAt 与 CreatedBy（来自 ICurrentUser.UserId）
    /// </summary>
    [Fact]
    public async Task SavingChangesAsync_AddedAuditedEntity_ShouldSetCreationAudit()
    {
        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.Setup(u => u.UserId).Returns("user-001");
        var context = CreateContext(new NoelleAuditInterceptor(currentUserMock.Object));

        var entity = new TestAuditedEntity();
        context.AuditedEntities.Add(entity);

        await context.SaveChangesAsync();

        Assert.NotEqual(default, entity.CreatedAt);
        Assert.Equal("user-001", entity.CreatedBy);
        Assert.Null(entity.LastModifiedAt); // 新增不填充修改审计
        Assert.Null(entity.LastModifiedBy);
    }

    /// <summary>
    /// 当前用户无 Id 时 CreatedBy 应为 null
    /// </summary>
    [Fact]
    public async Task SavingChangesAsync_CurrentUserWithoutId_ShouldSetCreatedByNull()
    {
        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.Setup(u => u.UserId).Returns((string?)null);
        var context = CreateContext(new NoelleAuditInterceptor(currentUserMock.Object));

        var entity = new TestAuditedEntity { CreatedBy = "old-value" };
        context.AuditedEntities.Add(entity);

        await context.SaveChangesAsync();

        Assert.Null(entity.CreatedBy);
    }

    /// <summary>
    /// 修改审计实体应填充 LastModifiedAt 与 LastModifiedBy
    /// </summary>
    [Fact]
    public async Task SavingChangesAsync_ModifiedAuditedEntity_ShouldSetModificationAudit()
    {
        var currentUserMock = new Mock<ICurrentUser>();
        currentUserMock.Setup(u => u.UserId).Returns("user-003");
        var context = CreateContext(new NoelleAuditInterceptor(currentUserMock.Object));

        var entity = new TestAuditedEntity();
        context.AuditedEntities.Add(entity);
        await context.SaveChangesAsync();

        context.Entry(entity).State = EntityState.Modified;
        await context.SaveChangesAsync();

        Assert.NotNull(entity.LastModifiedAt);
        Assert.Equal("user-003", entity.LastModifiedBy);
    }

    /// <summary>
    /// 非审计实体不应被修改（同步路径代表）
    /// </summary>
    [Fact]
    public void SavingChanges_NonAuditedEntity_ShouldNotBeAffected()
    {
        var context = CreateContext(new NoelleAuditInterceptor(new Mock<ICurrentUser>().Object));

        context.NonAuditEntities.Add(new TestNonAuditEntity { Name = "plain" });

        context.SaveChanges();
    }

    private static AuditTestDbContext CreateContext(NoelleAuditInterceptor interceptor)
    {
        var options = new DbContextOptionsBuilder<AuditTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AuditTestDbContext(options, interceptor);
    }
}

/// <summary>
/// 审计测试实体（实现 IAudited）
/// </summary>
public class TestAuditedEntity : IAudited
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}

/// <summary>
/// 非审计测试实体
/// </summary>
public class TestNonAuditEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

/// <summary>
/// 审计测试 DbContext
/// </summary>
public class AuditTestDbContext : DbContext
{
    private readonly NoelleAuditInterceptor _interceptor;

    public AuditTestDbContext(DbContextOptions<AuditTestDbContext> options, NoelleAuditInterceptor interceptor)
        : base(options)
    {
        _interceptor = interceptor;
    }

    public DbSet<TestAuditedEntity> AuditedEntities { get; set; } = null!;
    public DbSet<TestNonAuditEntity> NonAuditEntities { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_interceptor);
    }
}
