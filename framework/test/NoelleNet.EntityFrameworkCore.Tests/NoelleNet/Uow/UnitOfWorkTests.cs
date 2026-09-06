using Microsoft.EntityFrameworkCore;
using Moq;

namespace NoelleNet.Uow;

/// <summary>
/// <see cref="UnitOfWork"/> 的契约测试：SaveChangesAsync 的转发与异常传播
/// </summary>
public class UnitOfWorkTests
{
    /// <summary>
    /// SaveChangesAsync 应转发到 DbContext 并返回影响行数
    /// </summary>
    [Fact]
    public async Task SaveChangesAsync_ShouldForwardToDbContext()
    {
        var dbContextMock = new Mock<DbContext>();
        dbContextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        var result = await new UnitOfWork(dbContextMock.Object).SaveChangesAsync();

        Assert.Equal(5, result);
        dbContextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// CancellationToken 应透传给 DbContext
    /// </summary>
    [Fact]
    public async Task SaveChangesAsync_ShouldPassCancellationToken()
    {
        var dbContextMock = new Mock<DbContext>();
        var cts = new CancellationTokenSource();

        await new UnitOfWork(dbContextMock.Object).SaveChangesAsync(cts.Token);

        dbContextMock.Verify(c => c.SaveChangesAsync(cts.Token), Times.Once);
    }

    /// <summary>
    /// DbContext 抛出的异常应原样传播
    /// </summary>
    [Fact]
    public async Task SaveChangesAsync_WhenDbContextThrows_ShouldPropagate()
    {
        var dbContextMock = new Mock<DbContext>();
        dbContextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Test error"));

        await Assert.ThrowsAsync<DbUpdateException>(() => new UnitOfWork(dbContextMock.Object).SaveChangesAsync());
    }
}
