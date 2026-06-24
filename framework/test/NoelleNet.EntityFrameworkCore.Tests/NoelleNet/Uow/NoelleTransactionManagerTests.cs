using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data;

namespace NoelleNet.Uow;

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

/// <summary>
/// <see cref="NoelleTransactionManager"/> 的单元测试
/// </summary>
public class NoelleTransactionManagerTests
{
    private TransactionTestDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TransactionTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TransactionTestDbContext(options);
    }

    private NoelleTransactionManager CreateManager(TransactionTestDbContext dbContext)
    {
        return new NoelleTransactionManager(dbContext);
    }

    #region 构造函数

    [Fact]
    public void Constructor_DbContextIsNull_ShouldThrowArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new NoelleTransactionManager(null!));
        Assert.Equal("dbContext", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidDbContext_ShouldCreateInstance()
    {
        using var dbContext = CreateDbContext();
        var manager = new NoelleTransactionManager(dbContext);

        Assert.NotNull(manager);
    }

    #endregion

    #region HasActiveTransaction / TransactionId - 初始状态

    [Fact]
    public void HasActiveTransaction_DefaultState_ShouldReturnFalse()
    {
        using var dbContext = CreateDbContext();
        var manager = CreateManager(dbContext);

        Assert.False(manager.HasActiveTransaction);
    }

    [Fact]
    public void TransactionId_DefaultState_ShouldReturnNull()
    {
        using var dbContext = CreateDbContext();
        var manager = CreateManager(dbContext);

        Assert.Null(manager.TransactionId);
    }

    #endregion

    #region BeginAsync

    [Fact]
    public async Task BeginAsync_NoActiveTransaction_ShouldBeginNewTransaction()
    {
        using var dbContext = CreateDbContext();
        var manager = CreateManager(dbContext);

        await manager.BeginAsync();

        Assert.True(manager.HasActiveTransaction);
        Assert.NotNull(manager.TransactionId);
    }

    [Fact]
    public async Task BeginAsync_AlreadyHasActiveTransaction_ShouldThrowInvalidOperationException()
    {
        using var dbContext = CreateDbContext();
        var manager = CreateManager(dbContext);

        await manager.BeginAsync();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => manager.BeginAsync());
        Assert.Contains("活动的事务", exception.Message);
    }

    [Fact]
    public async Task BeginAsync_WithIsolationLevel_ShouldSucceed()
    {
        using var dbContext = CreateDbContext();
        var manager = CreateManager(dbContext);
        var expectedLevel = IsolationLevel.Serializable;

        await manager.BeginAsync(expectedLevel);

        Assert.True(manager.HasActiveTransaction);
        Assert.NotNull(manager.TransactionId);
    }

    [Fact]
    public async Task BeginAsync_WithIsolationLevel_AlreadyActive_ShouldThrowInvalidOperationException()
    {
        using var dbContext = CreateDbContext();
        var manager = CreateManager(dbContext);

        await manager.BeginAsync();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => manager.BeginAsync(IsolationLevel.Serializable));
        Assert.Contains("活动的事务", exception.Message);
    }

    [Fact]
    public async Task BeginAsync_WithCancellationToken_ShouldSucceed()
    {
        using var dbContext = CreateDbContext();
        var manager = CreateManager(dbContext);
        var cts = new CancellationTokenSource();

        await manager.BeginAsync(cts.Token);

        Assert.True(manager.HasActiveTransaction);
    }

    #endregion

    #region CommitAsync

    [Fact]
    public async Task CommitAsync_NoActiveTransaction_ShouldThrowInvalidOperationException()
    {
        using var dbContext = CreateDbContext();
        var manager = CreateManager(dbContext);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => manager.CommitAsync());
        Assert.Contains("没有活动事务", exception.Message);
    }

    [Fact]
    public async Task CommitAsync_ShouldCommitAndClearTransaction()
    {
        using var dbContext = CreateDbContext();
        var manager = CreateManager(dbContext);

        await manager.BeginAsync();
        Assert.True(manager.HasActiveTransaction);

        await manager.CommitAsync();

        Assert.False(manager.HasActiveTransaction);
    }

    [Fact]
    public async Task CommitAsync_ShouldClearCurrentTransaction()
    {
        using var dbContext = CreateDbContext();
        var manager = CreateManager(dbContext);

        await manager.BeginAsync();
        await manager.CommitAsync();

        Assert.False(manager.HasActiveTransaction);
        Assert.Null(manager.TransactionId);
    }

    [Fact]
    public async Task CommitAsync_WithCancellationToken_ShouldSucceed()
    {
        using var dbContext = CreateDbContext();
        var manager = CreateManager(dbContext);
        var cts = new CancellationTokenSource();

        await manager.BeginAsync();
        await manager.CommitAsync(cts.Token);

        Assert.False(manager.HasActiveTransaction);
    }

    #endregion

    #region RollbackAsync

    [Fact]
    public async Task RollbackAsync_NoActiveTransaction_ShouldThrowInvalidOperationException()
    {
        using var dbContext = CreateDbContext();
        var manager = CreateManager(dbContext);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => manager.RollbackAsync());
        Assert.Contains("没有活动事务", exception.Message);
    }

    [Fact]
    public async Task RollbackAsync_ShouldRollbackAndClearTransaction()
    {
        using var dbContext = CreateDbContext();
        var manager = CreateManager(dbContext);

        await manager.BeginAsync();
        await manager.RollbackAsync();

        Assert.False(manager.HasActiveTransaction);
    }

    [Fact]
    public async Task RollbackAsync_ShouldClearCurrentTransaction()
    {
        using var dbContext = CreateDbContext();
        var manager = CreateManager(dbContext);

        await manager.BeginAsync();
        await manager.RollbackAsync();

        Assert.False(manager.HasActiveTransaction);
        Assert.Null(manager.TransactionId);
    }

    [Fact]
    public async Task RollbackAsync_WithCancellationToken_ShouldSucceed()
    {
        using var dbContext = CreateDbContext();
        var manager = CreateManager(dbContext);
        var cts = new CancellationTokenSource();

        await manager.BeginAsync();
        await manager.RollbackAsync(cts.Token);

        Assert.False(manager.HasActiveTransaction);
    }

    #endregion

    #region Dispose

    [Fact]
    public void Dispose_NoActiveTransaction_ShouldNotThrow()
    {
        using var dbContext = CreateDbContext();
        var manager = CreateManager(dbContext);

        var exception = Record.Exception(() => manager.Dispose());
        Assert.Null(exception);
    }

    [Fact]
    public async Task Dispose_WithActiveTransaction_ShouldDisposeTransaction()
    {
        using var dbContext = CreateDbContext();
        var manager = CreateManager(dbContext);

        await manager.BeginAsync();
        Assert.True(manager.HasActiveTransaction);

        manager.Dispose();

        Assert.False(manager.HasActiveTransaction);
    }

    #endregion

    #region DisposeAsync

    [Fact]
    public async Task DisposeAsync_NoActiveTransaction_ShouldNotThrow()
    {
        using var dbContext = CreateDbContext();
        var manager = CreateManager(dbContext);

        await manager.DisposeAsync();
    }

    [Fact]
    public async Task DisposeAsync_WithActiveTransaction_ShouldDisposeTransaction()
    {
        using var dbContext = CreateDbContext();
        var manager = CreateManager(dbContext);

        await manager.BeginAsync();
        await manager.DisposeAsync();

        Assert.False(manager.HasActiveTransaction);
    }

    #endregion

    #region 生命周期

    [Fact]
    public async Task FullTransactionLifecycle_BeginCommit_ShouldSucceed()
    {
        using var dbContext = CreateDbContext();
        var manager = CreateManager(dbContext);

        Assert.False(manager.HasActiveTransaction);
        Assert.Null(manager.TransactionId);

        await manager.BeginAsync();

        Assert.True(manager.HasActiveTransaction);
        Assert.NotNull(manager.TransactionId);

        await manager.CommitAsync();

        Assert.False(manager.HasActiveTransaction);
        Assert.Null(manager.TransactionId);
    }

    [Fact]
    public async Task FullTransactionLifecycle_BeginRollback_ShouldSucceed()
    {
        using var dbContext = CreateDbContext();
        var manager = CreateManager(dbContext);

        await manager.BeginAsync();
        await manager.RollbackAsync();

        Assert.False(manager.HasActiveTransaction);
    }

    [Fact]
    public async Task BeginAfterCommit_ShouldAllowNewTransaction()
    {
        using var dbContext = CreateDbContext();
        var manager = CreateManager(dbContext);

        await manager.BeginAsync();
        await manager.CommitAsync();

        // 新的事务应该可以开始
        await manager.BeginAsync();

        Assert.True(manager.HasActiveTransaction);
        Assert.NotNull(manager.TransactionId);
    }

    #endregion
}
