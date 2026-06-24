using Microsoft.EntityFrameworkCore;
using Moq;

namespace NoelleNet.Uow;

/// <summary>
/// <see cref="UnitOfWork"/> 的单元测试
/// </summary>
public class UnitOfWorkTests
{
    #region 构造函数

    [Fact]
    public void Constructor_DbContextIsNull_ShouldThrowArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new UnitOfWork(null!));
        Assert.Equal("dbContext", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidDbContext_ShouldCreateInstance()
    {
        var dbContext = new Mock<DbContext>().Object;
        var uow = new UnitOfWork(dbContext);

        Assert.NotNull(uow);
        Assert.IsAssignableFrom<IUnitOfWork>(uow);
    }

    #endregion

    #region SaveChangesAsync

    [Fact]
    public async Task SaveChangesAsync_ShouldCallDbContextSaveChangesAsync()
    {
        var dbContextMock = new Mock<DbContext>();
        dbContextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        var uow = new UnitOfWork(dbContextMock.Object);

        var result = await uow.SaveChangesAsync();

        Assert.Equal(5, result);
        dbContextMock.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task SaveChangesAsync_WithCancellationToken_ShouldPassToDbContext()
    {
        var dbContextMock = new Mock<DbContext>();
        var cts = new CancellationTokenSource();
        dbContextMock.Setup(c => c.SaveChangesAsync(cts.Token))
            .ReturnsAsync(3);

        var uow = new UnitOfWork(dbContextMock.Object);

        var result = await uow.SaveChangesAsync(cts.Token);

        Assert.Equal(3, result);
        dbContextMock.Verify(c => c.SaveChangesAsync(cts.Token), Times.Once);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenDbContextReturnsZero_ShouldReturnZero()
    {
        var dbContextMock = new Mock<DbContext>();
        dbContextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var uow = new UnitOfWork(dbContextMock.Object);

        var result = await uow.SaveChangesAsync();

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenDbContextThrows_ShouldPropagateException()
    {
        var dbContextMock = new Mock<DbContext>();
        dbContextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Test error"));

        var uow = new UnitOfWork(dbContextMock.Object);

        await Assert.ThrowsAsync<DbUpdateException>(() => uow.SaveChangesAsync());
    }

    #endregion
}
