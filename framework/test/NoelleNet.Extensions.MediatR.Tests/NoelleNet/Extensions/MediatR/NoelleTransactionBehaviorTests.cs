using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using NoelleNet.Uow;

namespace NoelleNet.Extensions.MediatR;

/// <summary>
/// <see cref="NoelleTransactionBehavior{TRequest, TResponse}"/> 的单元测试
/// </summary>
public class NoelleTransactionBehaviorTests
{
    #region Constructor

    /// <summary>
    /// 传入 null 的 ILogger 时应抛出 ArgumentNullException
    /// </summary>
    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var mockUow = new Mock<IUnitOfWork>();
        var mockTm = new Mock<ITransactionManager>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => new NoelleTransactionBehavior<TestTransactionRequest, TestTransactionResponse>(
                null!, dbContext, mockUow.Object, mockTm.Object));
    }

    /// <summary>
    /// 传入 null 的 DbContext 时应抛出 ArgumentNullException
    /// </summary>
    [Fact]
    public void Constructor_WithNullDbContext_ShouldThrowArgumentNullException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<NoelleTransactionBehavior<TestTransactionRequest, TestTransactionResponse>>>();
        var mockUow = new Mock<IUnitOfWork>();
        var mockTm = new Mock<ITransactionManager>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => new NoelleTransactionBehavior<TestTransactionRequest, TestTransactionResponse>(
                mockLogger.Object, null!, mockUow.Object, mockTm.Object));
    }

    /// <summary>
    /// 传入 null 的 IUnitOfWork 时应抛出 ArgumentNullException
    /// </summary>
    [Fact]
    public void Constructor_WithNullUnitOfWork_ShouldThrowArgumentNullException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<NoelleTransactionBehavior<TestTransactionRequest, TestTransactionResponse>>>();
        using var dbContext = CreateDbContext();
        var mockTm = new Mock<ITransactionManager>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => new NoelleTransactionBehavior<TestTransactionRequest, TestTransactionResponse>(
                mockLogger.Object, dbContext, null!, mockTm.Object));
    }

    /// <summary>
    /// 传入 null 的 ITransactionManager 时应抛出 ArgumentNullException
    /// </summary>
    [Fact]
    public void Constructor_WithNullTransactionManager_ShouldThrowArgumentNullException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<NoelleTransactionBehavior<TestTransactionRequest, TestTransactionResponse>>>();
        using var dbContext = CreateDbContext();
        var mockUow = new Mock<IUnitOfWork>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => new NoelleTransactionBehavior<TestTransactionRequest, TestTransactionResponse>(
                mockLogger.Object, dbContext, mockUow.Object, null!));
    }

    /// <summary>
    /// 传入所有有效参数时应成功创建实例
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<NoelleTransactionBehavior<TestTransactionRequest, TestTransactionResponse>>>();
        using var dbContext = CreateDbContext();
        var mockUow = new Mock<IUnitOfWork>();
        var mockTm = new Mock<ITransactionManager>();

        // Act
        var behavior = new NoelleTransactionBehavior<TestTransactionRequest, TestTransactionResponse>(
            mockLogger.Object, dbContext, mockUow.Object, mockTm.Object);

        // Assert
        Assert.NotNull(behavior);
    }

    #endregion

    #region Interface Implementation

    /// <summary>
    /// NoelleTransactionBehavior 应实现 IPipelineBehavior&lt;,&gt; 接口
    /// </summary>
    [Fact]
    public void ShouldImplementIPipelineBehavior()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<NoelleTransactionBehavior<TestTransactionRequest, TestTransactionResponse>>>();
        using var dbContext = CreateDbContext();
        var mockUow = new Mock<IUnitOfWork>();
        var mockTm = new Mock<ITransactionManager>();

        // Act
        var behavior = new NoelleTransactionBehavior<TestTransactionRequest, TestTransactionResponse>(
            mockLogger.Object, dbContext, mockUow.Object, mockTm.Object);

        // Assert
        Assert.IsAssignableFrom<IPipelineBehavior<TestTransactionRequest, TestTransactionResponse?>>(behavior);
    }

    #endregion

    #region Handle - Active Transaction

    /// <summary>
    /// 当存在活跃事务时，应跳过事务控制直接调用 next 委托
    /// </summary>
    [Fact]
    public async Task Handle_WhenActiveTransactionExists_ShouldSkipTransactionAndCallNext()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<NoelleTransactionBehavior<TestTransactionRequest, TestTransactionResponse>>>();
        using var dbContext = CreateDbContext();
        var mockUow = new Mock<IUnitOfWork>();
        var mockTm = new Mock<ITransactionManager>();
        mockTm.Setup(tm => tm.HasActiveTransaction).Returns(true);

        var behavior = new NoelleTransactionBehavior<TestTransactionRequest, TestTransactionResponse>(
            mockLogger.Object, dbContext, mockUow.Object, mockTm.Object);

        var request = new TestTransactionRequest { Id = 1 };
        var expectedResponse = new TestTransactionResponse { Success = true };
        var nextCalled = false;

        // Act
        var response = await behavior.Handle(request, _ =>
        {
            nextCalled = true;
            return Task.FromResult<TestTransactionResponse?>(expectedResponse);
        }, CancellationToken.None);

        // Assert
        Assert.True(nextCalled);
        Assert.NotNull(response);
        Assert.True(response.Success);

        // 验证未执行事务开始/提交/保存变更
        mockTm.Verify(tm => tm.BeginAsync(It.IsAny<CancellationToken>()), Times.Never);
        mockUow.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        mockTm.Verify(tm => tm.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Handle - Normal Flow

    /// <summary>
    /// 正常流程应依次执行：开始事务、调用 next、保存变更、提交事务
    /// </summary>
    [Fact]
    public async Task Handle_ShouldBeginTransactionSaveChangesAndCommit()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<NoelleTransactionBehavior<TestTransactionRequest, TestTransactionResponse>>>();
        using var dbContext = CreateDbContext();
        var mockUow = new Mock<IUnitOfWork>();
        var mockTm = new Mock<ITransactionManager>();

        var transactionId = Guid.NewGuid();
        mockTm.Setup(tm => tm.HasActiveTransaction).Returns(false);
        mockTm.Setup(tm => tm.TransactionId).Returns(transactionId);
        mockTm.Setup(tm => tm.BeginAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockTm.Setup(tm => tm.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockUow.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var behavior = new NoelleTransactionBehavior<TestTransactionRequest, TestTransactionResponse>(
            mockLogger.Object, dbContext, mockUow.Object, mockTm.Object);

        var request = new TestTransactionRequest { Id = 42 };
        var expectedResponse = new TestTransactionResponse { Success = true };
        var nextCalled = false;

        // Act
        var response = await behavior.Handle(request, _ =>
        {
            nextCalled = true;
            return Task.FromResult<TestTransactionResponse?>(expectedResponse);
        }, CancellationToken.None);

        // Assert
        Assert.True(nextCalled);
        Assert.NotNull(response);
        Assert.True(response.Success);

        // 验证执行顺序：Begin -> SaveChanges -> Commit
        mockTm.Verify(tm => tm.BeginAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockUow.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockTm.Verify(tm => tm.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// 在事务执行期间应记录日志
    /// </summary>
    [Fact]
    public async Task Handle_ShouldLogTransactionInfo()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<NoelleTransactionBehavior<TestTransactionRequest, TestTransactionResponse>>>();
        using var dbContext = CreateDbContext();
        var mockUow = new Mock<IUnitOfWork>();
        var mockTm = new Mock<ITransactionManager>();

        var transactionId = Guid.NewGuid();
        mockTm.Setup(tm => tm.HasActiveTransaction).Returns(false);
        mockTm.Setup(tm => tm.TransactionId).Returns(transactionId);
        mockTm.Setup(tm => tm.BeginAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockTm.Setup(tm => tm.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockUow.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var behavior = new NoelleTransactionBehavior<TestTransactionRequest, TestTransactionResponse>(
            mockLogger.Object, dbContext, mockUow.Object, mockTm.Object);

        var request = new TestTransactionRequest { Id = 100 };

        // Act
        await behavior.Handle(request, _ => Task.FromResult<TestTransactionResponse?>(new TestTransactionResponse { Success = true }), CancellationToken.None);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) => state.ToString()!.Contains("开始事务")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) => state.ToString()!.Contains("事务完成")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region Handle - Exception

    /// <summary>
    /// 当 next 委托抛出异常时，应回滚事务并重新抛出异常
    /// </summary>
    [Fact]
    public async Task Handle_WhenNextThrows_ShouldRollbackAndRethrow()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<NoelleTransactionBehavior<TestTransactionRequest, TestTransactionResponse>>>();
        using var dbContext = CreateDbContext();
        var mockUow = new Mock<IUnitOfWork>();
        var mockTm = new Mock<ITransactionManager>();

        var transactionId = Guid.NewGuid();
        var hasActiveTransaction = false;
        mockTm.Setup(tm => tm.HasActiveTransaction).Returns(() => hasActiveTransaction);
        mockTm.Setup(tm => tm.TransactionId).Returns(transactionId);
        mockTm.Setup(tm => tm.BeginAsync(It.IsAny<CancellationToken>()))
            .Callback(() => hasActiveTransaction = true)
            .Returns(Task.CompletedTask);
        mockTm.Setup(tm => tm.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var behavior = new NoelleTransactionBehavior<TestTransactionRequest, TestTransactionResponse>(
            mockLogger.Object, dbContext, mockUow.Object, mockTm.Object);

        var request = new TestTransactionRequest { Id = 500 };
        var expectedException = new InvalidOperationException("Transaction failure");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => behavior.Handle(request, _ => throw expectedException, CancellationToken.None));

        Assert.Equal("Transaction failure", exception.Message);

        // 验证执行了回滚
        mockTm.Verify(tm => tm.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);

        // 验证没有执行提交
        mockTm.Verify(tm => tm.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        mockUow.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// 当发生异常时，应记录错误日志
    /// </summary>
    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldLogError()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<NoelleTransactionBehavior<TestTransactionRequest, TestTransactionResponse>>>();
        using var dbContext = CreateDbContext();
        var mockUow = new Mock<IUnitOfWork>();
        var mockTm = new Mock<ITransactionManager>();

        mockTm.Setup(tm => tm.HasActiveTransaction).Returns(false);
        mockTm.Setup(tm => tm.BeginAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockTm.Setup(tm => tm.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var behavior = new NoelleTransactionBehavior<TestTransactionRequest, TestTransactionResponse>(
            mockLogger.Object, dbContext, mockUow.Object, mockTm.Object);

        var request = new TestTransactionRequest { Id = 999 };

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => behavior.Handle(request, _ => throw new InvalidOperationException("Error"), CancellationToken.None));

        // Assert — 验证记录了错误日志
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) => state.ToString()!.Contains("事务处理错误")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    /// <summary>
    /// 当 next 返回 null 响应时应正常处理
    /// </summary>
    [Fact]
    public async Task Handle_WhenNextReturnsNull_ShouldCompleteSuccessfully()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<NoelleTransactionBehavior<TestTransactionRequest, TestTransactionResponse>>>();
        using var dbContext = CreateDbContext();
        var mockUow = new Mock<IUnitOfWork>();
        var mockTm = new Mock<ITransactionManager>();

        mockTm.Setup(tm => tm.HasActiveTransaction).Returns(false);
        mockTm.Setup(tm => tm.BeginAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockTm.Setup(tm => tm.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockUow.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var behavior = new NoelleTransactionBehavior<TestTransactionRequest, TestTransactionResponse>(
            mockLogger.Object, dbContext, mockUow.Object, mockTm.Object);

        var request = new TestTransactionRequest { Id = 7 };

        // Act
        var response = await behavior.Handle(request, _ => Task.FromResult<TestTransactionResponse?>(null), CancellationToken.None);

        // Assert
        Assert.Null(response);
        mockTm.Verify(tm => tm.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Helpers

    /// <summary>
    /// 创建使用 InMemory 数据库的 DbContext
    /// </summary>
    private static TestDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new TestDbContext(options);
    }

    #endregion
}

#region Test Types

/// <summary>
/// 测试用事务请求
/// </summary>
public class TestTransactionRequest
{
    public int Id { get; set; }
}

/// <summary>
/// 测试用事务响应
/// </summary>
public class TestTransactionResponse
{
    public bool Success { get; set; }
}

/// <summary>
/// 测试用 DbContext
/// </summary>
public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public DbSet<TestEntity> TestEntities => Set<TestEntity>();
}

/// <summary>
/// 测试用实体
/// </summary>
public class TestEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

#endregion
