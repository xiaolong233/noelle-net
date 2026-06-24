using Microsoft.EntityFrameworkCore;
using Moq;
using NoelleNet.Auditing;
using NoelleNet.Security;

namespace NoelleNet.Auditing.EntityFrameworkCore;

#region Test Entities

/// <summary>
/// 测试实体，实现 IHasCreatedAt 接口
/// </summary>
public class TestCreatedAtEntity : IHasCreatedAt
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 测试实体，实现 IMayHaveCreator 接口
/// </summary>
public class TestMayHaveCreatorEntity : IMayHaveCreator
{
    public int Id { get; set; }
    public string? CreatedBy { get; set; }
}

/// <summary>
/// 测试实体，实现 IModificationAudited 接口
/// </summary>
public class TestModificationAuditedEntity : IModificationAudited
{
    public int Id { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}

/// <summary>
/// 测试实体，实现 IAudited 接口（同时包含创建和修改审计）
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
/// 测试实体，同时实现 IHasCreatedAt 和 IMayHaveCreator（但不实现 IAudited）
/// </summary>
public class TestCreationAuditedEntity : IHasCreatedAt, IMayHaveCreator
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

/// <summary>
/// 非审计实体（用于验证拦截器不会错误处理普通实体）
/// </summary>
public class TestNonAuditEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

#endregion

/// <summary>
/// <see cref="NoelleAuditInterceptor"/> 的单元测试
/// </summary>
public class NoelleAuditInterceptorTests
{
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly NoelleAuditInterceptor _interceptor;

    public NoelleAuditInterceptorTests()
    {
        _currentUserMock = new Mock<ICurrentUser>();
        _interceptor = new NoelleAuditInterceptor(_currentUserMock.Object);
    }

    #region 构造函数

    [Fact]
    public void Constructor_CurrentUserIsNull_ShouldThrowArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new NoelleAuditInterceptor(null!));
        Assert.Equal("currentUser", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidCurrentUser_ShouldCreateInstance()
    {
        var user = new Mock<ICurrentUser>().Object;
        var interceptor = new NoelleAuditInterceptor(user);

        Assert.NotNull(interceptor);
    }

    #endregion

    #region SavingChanges

    [Fact]
    public void SavingChanges_NoAuditEntities_ShouldNotModifyEntities()
    {
        var options = new DbContextOptionsBuilder<AuditTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new AuditTestDbContext(options, _interceptor);
        var entity = new TestNonAuditEntity();
        context.NonAuditEntities.Add(entity);

        var result = context.SaveChanges();

        Assert.Equal(1, result);
    }

    [Fact]
    public void SavingChanges_AddedEntity_ShouldSetCreatedAtAndCreatedBy()
    {
        var userId = "user-001";
        _currentUserMock.Setup(u => u.Id).Returns(userId);

        var options = new DbContextOptionsBuilder<AuditTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new AuditTestDbContext(options, _interceptor);
        var entity = new TestCreationAuditedEntity();
        context.CreationAuditedEntities.Add(entity);

        context.SaveChanges();

        Assert.NotEqual(default, entity.CreatedAt);
        Assert.Equal(userId, entity.CreatedBy);
    }

    [Fact]
    public void SavingChanges_AddedAuditedEntity_ShouldSetAllCreationAuditFields()
    {
        var userId = "user-002";
        _currentUserMock.Setup(u => u.Id).Returns(userId);

        var options = new DbContextOptionsBuilder<AuditTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new AuditTestDbContext(options, _interceptor);
        var entity = new TestAuditedEntity();
        context.AuditedEntities.Add(entity);

        context.SaveChanges();

        Assert.NotEqual(default, entity.CreatedAt);
        Assert.Equal(userId, entity.CreatedBy);
        Assert.Null(entity.LastModifiedAt);
        Assert.Null(entity.LastModifiedBy);
    }

    [Fact]
    public void SavingChanges_ModifiedEntity_ShouldSetModificationAuditFields()
    {
        var userId = "user-003";
        _currentUserMock.Setup(u => u.Id).Returns(userId);

        var options = new DbContextOptionsBuilder<AuditTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new AuditTestDbContext(options, _interceptor);
        var entity = new TestModificationAuditedEntity { LastModifiedAt = null, LastModifiedBy = null };
        context.ModificationAuditedEntities.Add(entity);
        context.SaveChanges();

        // 标记为已修改
        context.Entry(entity).State = EntityState.Modified;
        context.SaveChanges();

        Assert.NotEqual(default, entity.LastModifiedAt);
        Assert.Equal(userId, entity.LastModifiedBy);
    }

    [Fact]
    public void SavingChanges_ModifiedAuditedEntity_ShouldSetModificationFields()
    {
        var userId = "user-004";
        _currentUserMock.Setup(u => u.Id).Returns(userId);

        var options = new DbContextOptionsBuilder<AuditTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new AuditTestDbContext(options, _interceptor);
        var entity = new TestAuditedEntity();
        context.AuditedEntities.Add(entity);
        context.SaveChanges();

        context.Entry(entity).State = EntityState.Modified;
        context.SaveChanges();

        Assert.NotNull(entity.LastModifiedAt);
        Assert.Equal(userId, entity.LastModifiedBy);
    }

    [Fact]
    public void SavingChanges_CurrentUserHasNoId_ShouldSetCreatedByAsNull()
    {
        _currentUserMock.Setup(u => u.Id).Returns((string?)null);

        var options = new DbContextOptionsBuilder<AuditTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new AuditTestDbContext(options, _interceptor);
        var entity = new TestCreationAuditedEntity { CreatedBy = "old-value" };
        context.CreationAuditedEntities.Add(entity);

        context.SaveChanges();

        Assert.NotEqual(default, entity.CreatedAt);
        Assert.Null(entity.CreatedBy);
    }

    [Fact]
    public void SavingChanges_NoMatchingAuditEntities_ShouldReturnOriginalResult()
    {
        var options = new DbContextOptionsBuilder<AuditTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new AuditTestDbContext(options, _interceptor);
        context.NonAuditEntities.Add(new TestNonAuditEntity());

        var result = context.SaveChanges();

        Assert.Equal(1, result);
    }

    #endregion

    #region SavingChangesAsync

    [Fact]
    public async Task SavingChangesAsync_AddedEntity_ShouldSetCreatedAtAndCreatedBy()
    {
        var userId = "async-user-001";
        _currentUserMock.Setup(u => u.Id).Returns(userId);

        var options = new DbContextOptionsBuilder<AuditTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new AuditTestDbContext(options, _interceptor);
        var entity = new TestCreationAuditedEntity();
        context.CreationAuditedEntities.Add(entity);

        await context.SaveChangesAsync();

        Assert.NotEqual(default, entity.CreatedAt);
        Assert.Equal(userId, entity.CreatedBy);
    }

    [Fact]
    public async Task SavingChangesAsync_ModifiedEntity_ShouldSetModificationFields()
    {
        var userId = "async-user-002";
        _currentUserMock.Setup(u => u.Id).Returns(userId);

        var options = new DbContextOptionsBuilder<AuditTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new AuditTestDbContext(options, _interceptor);
        var entity = new TestModificationAuditedEntity();
        context.ModificationAuditedEntities.Add(entity);
        await context.SaveChangesAsync();

        context.Entry(entity).State = EntityState.Modified;
        await context.SaveChangesAsync();

        Assert.NotNull(entity.LastModifiedAt);
        Assert.Equal(userId, entity.LastModifiedBy);
    }

    #endregion
}

/// <summary>
/// 用于审计测试的 DbContext
/// </summary>
public class AuditTestDbContext : DbContext
{
    private readonly NoelleAuditInterceptor _interceptor;

    public DbSet<TestNonAuditEntity> NonAuditEntities { get; set; } = null!;
    public DbSet<TestCreationAuditedEntity> CreationAuditedEntities { get; set; } = null!;
    public DbSet<TestModificationAuditedEntity> ModificationAuditedEntities { get; set; } = null!;
    public DbSet<TestAuditedEntity> AuditedEntities { get; set; } = null!;

    public AuditTestDbContext(DbContextOptions<AuditTestDbContext> options, NoelleAuditInterceptor interceptor)
        : base(options)
    {
        _interceptor = interceptor;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_interceptor);
    }
}
