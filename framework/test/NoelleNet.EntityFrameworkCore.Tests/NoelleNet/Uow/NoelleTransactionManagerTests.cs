using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data;

namespace NoelleNet.Uow;

/// <summary>
/// <see cref="NoelleTransactionManager"/> 的契约测试：事务生命周期、防重入与释放清理
/// </summary>
public class NoelleTransactionManagerTests
{
    private static TransactionTestDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TransactionTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TransactionTestDbContext(options);
    }

    /// <summary>
    /// 初始状态：无活动事务、无事务 Id
    /// </summary>
    [Fact]
    public void InitialState_ShouldHaveNoActiveTransaction()
    {
        using var dbContext = CreateDbContext();
        var manager = new NoelleTransactionManager(dbContext);

        Assert.False(manager.HasActiveTransaction);
        Assert.Null(manager.TransactionId);
    }

    /// <summary>
    /// 开启事务后 HasActiveTransaction 与 TransactionId 生效；重复开启（含指定隔离级别）应抛 InvalidOperationException
    /// </summary>
    [Fact]
    public async Task BeginAsync_ShouldStartAndPreventDuplicateBegin()
    {
        using var dbContext = CreateDbContext();
        var manager = new NoelleTransactionManager(dbContext);

        await manager.BeginAsync();
        Assert.True(manager.HasActiveTransaction);
        Assert.NotNull(manager.TransactionId);

        await Assert.ThrowsAsync<InvalidOperationException>(() => manager.BeginAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(() => manager.BeginAsync(IsolationLevel.Serializable));
    }

    /// <summary>
    /// 提交应清除活动事务；无事务提交应抛 InvalidOperationException
    /// </summary>
    [Fact]
    public async Task CommitAsync_ShouldClearTransactionOrThrowWhenNone()
    {
        using var dbContext = CreateDbContext();
        var manager = new NoelleTransactionManager(dbContext);

        await Assert.ThrowsAsync<InvalidOperationException>(() => manager.CommitAsync());

        await manager.BeginAsync();
        await manager.CommitAsync();

        Assert.False(manager.HasActiveTransaction);
        Assert.Null(manager.TransactionId);
    }

    /// <summary>
    /// 回滚应清除活动事务；无事务回滚应抛 InvalidOperationException
    /// </summary>
    [Fact]
    public async Task RollbackAsync_ShouldClearTransactionOrThrowWhenNone()
    {
        using var dbContext = CreateDbContext();
        var manager = new NoelleTransactionManager(dbContext);

        await Assert.ThrowsAsync<InvalidOperationException>(() => manager.RollbackAsync());

        await manager.BeginAsync();
        await manager.RollbackAsync();

        Assert.False(manager.HasActiveTransaction);
        Assert.Null(manager.TransactionId);
    }

    /// <summary>
    /// 提交后可再次开启新事务（事务复用契约）
    /// </summary>
    [Fact]
    public async Task BeginAfterCommit_ShouldAllowNewTransaction()
    {
        using var dbContext = CreateDbContext();
        var manager = new NoelleTransactionManager(dbContext);

        await manager.BeginAsync();
        await manager.CommitAsync();
        await manager.BeginAsync();

        Assert.True(manager.HasActiveTransaction);
    }

    /// <summary>
    /// Dispose / DisposeAsync 应释放活动事务并清除状态；无事务时不应抛异常
    /// </summary>
    [Fact]
    public async Task Dispose_ShouldReleaseActiveTransaction()
    {
        using var dbContext = CreateDbContext();
        var manager = new NoelleTransactionManager(dbContext);

        manager.Dispose(); // 无事务释放不应抛异常

        await manager.BeginAsync();
        manager.Dispose();
        Assert.False(manager.HasActiveTransaction);

        await manager.BeginAsync();
        await manager.DisposeAsync();
        Assert.False(manager.HasActiveTransaction);
    }
}

/// <summary>
/// 用于事务测试的 DbContext（禁用事务不支持警告）
/// </summary>
public class TransactionTestDbContext : DbContext
{
    public TransactionTestDbContext(DbContextOptions<TransactionTestDbContext> options) : base(options) { }

    public DbSet<TransactionTestEntity> Entities { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
    }
}

/// <summary>
/// 用于事务测试的实体
/// </summary>
public class TransactionTestEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}
