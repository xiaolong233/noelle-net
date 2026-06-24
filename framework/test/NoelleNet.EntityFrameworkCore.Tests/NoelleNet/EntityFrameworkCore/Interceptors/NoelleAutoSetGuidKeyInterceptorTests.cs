using Microsoft.EntityFrameworkCore;
using Moq;
using NoelleNet.Ddd.Domain.Entities;
using NoelleNet.EntityFrameworkCore.Interceptors;

namespace NoelleNet.EntityFrameworkCore.Interceptors;

#region Test Entities

/// <summary>
/// 用于测试的 Guid 键实体（直接实现 IEntity&lt;Guid&gt; 并拥有公开可写的 Id）
/// 继承自 Entity&lt;Guid&gt; 但使用 new 关键字隐藏受保护的 Id setter
/// </summary>
public class TestGuidKeyEntityWithNew : Entity<Guid>
{
    private Guid _id;

    public string Name { get; set; } = "";

    public TestGuidKeyEntityWithNew() { }

    public TestGuidKeyEntityWithNew(Guid id) : base(id)
    {
        _id = id;
    }

    public new Guid Id
    {
        get => _id;
        set => _id = value;
    }
}

#endregion

/// <summary>
/// <see cref="NoelleAutoSetGuidKeyInterceptor"/> 的单元测试
/// 注：由于 Entity&lt;Guid&gt;.Id 使用 protected set，TrySetProperty 无法设置该属性。
/// 测试使用 new 关键字公开 Id setter 来验证拦截器的核心逻辑。
/// </summary>
public class NoelleAutoSetGuidKeyInterceptorTests
{
    #region 构造函数

    [Fact]
    public void Constructor_GuidGeneratorIsNull_ShouldThrowArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new NoelleAutoSetGuidKeyInterceptor(null!));
        Assert.Equal("guidGenerator", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidGenerator_ShouldCreateInstance()
    {
        var generator = new Mock<IGuidGenerator>().Object;
        var interceptor = new NoelleAutoSetGuidKeyInterceptor(generator);

        Assert.NotNull(interceptor);
    }

    #endregion

    #region SavingChanges

    [Fact]
    public void SavingChanges_GuidEntityWithEmptyId_ShouldCallGenerator()
    {
        var guidGeneratorMock = new Mock<IGuidGenerator>();
        var generatedGuid = Guid.NewGuid();
        guidGeneratorMock.Setup(g => g.Generate()).Returns(generatedGuid);
        var interceptor = new NoelleAutoSetGuidKeyInterceptor(guidGeneratorMock.Object);

        var options = new DbContextOptionsBuilder<GuidKeyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new GuidKeyDbContext(options, interceptor);
        var entity = new TestGuidKeyEntityWithNew { Name = "test" };
        context.GuidKeyEntities.Add(entity);

        context.SaveChanges();

        guidGeneratorMock.Verify(g => g.Generate(), Times.Once);
    }

    [Fact]
    public void SavingChanges_GuidEntityWithNonEmptyId_ShouldNotOverwriteId()
    {
        var guidGeneratorMock = new Mock<IGuidGenerator>();
        var interceptor = new NoelleAutoSetGuidKeyInterceptor(guidGeneratorMock.Object);
        var existingId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<GuidKeyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new GuidKeyDbContext(options, interceptor);
        var entity = new TestGuidKeyEntityWithNew(existingId) { Name = "test" };
        context.GuidKeyEntities.Add(entity);

        context.SaveChanges();

        Assert.Equal(existingId, entity.Id);
        guidGeneratorMock.Verify(g => g.Generate(), Times.Never);
    }

    [Fact]
    public void SavingChanges_NoEntities_ShouldNotThrow()
    {
        var guidGeneratorMock = new Mock<IGuidGenerator>();
        var interceptor = new NoelleAutoSetGuidKeyInterceptor(guidGeneratorMock.Object);

        var options = new DbContextOptionsBuilder<GuidKeyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new GuidKeyDbContext(options, interceptor);

        context.SaveChanges();

        guidGeneratorMock.Verify(g => g.Generate(), Times.Never);
    }

    #endregion

    #region SavingChangesAsync

    [Fact]
    public async Task SavingChangesAsync_GuidEntityWithEmptyId_ShouldCallGenerator()
    {
        var guidGeneratorMock = new Mock<IGuidGenerator>();
        var generatedGuid = Guid.NewGuid();
        guidGeneratorMock.Setup(g => g.Generate()).Returns(generatedGuid);
        var interceptor = new NoelleAutoSetGuidKeyInterceptor(guidGeneratorMock.Object);

        var options = new DbContextOptionsBuilder<GuidKeyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new GuidKeyDbContext(options, interceptor);
        var entity = new TestGuidKeyEntityWithNew { Name = "async-test" };
        context.GuidKeyEntities.Add(entity);

        await context.SaveChangesAsync();

        guidGeneratorMock.Verify(g => g.Generate(), Times.Once);
    }

    [Fact]
    public async Task SavingChangesAsync_GuidEntityWithNonEmptyId_ShouldNotCallGenerator()
    {
        var guidGeneratorMock = new Mock<IGuidGenerator>();
        var interceptor = new NoelleAutoSetGuidKeyInterceptor(guidGeneratorMock.Object);
        var existingId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<GuidKeyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new GuidKeyDbContext(options, interceptor);
        context.GuidKeyEntities.Add(new TestGuidKeyEntityWithNew(existingId) { Name = "test" });

        await context.SaveChangesAsync();

        guidGeneratorMock.Verify(g => g.Generate(), Times.Never);
    }

    [Fact]
    public async Task SavingChangesAsync_NoEntities_ShouldNotThrow()
    {
        var guidGeneratorMock = new Mock<IGuidGenerator>();
        var interceptor = new NoelleAutoSetGuidKeyInterceptor(guidGeneratorMock.Object);

        var options = new DbContextOptionsBuilder<GuidKeyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new GuidKeyDbContext(options, interceptor);

        await context.SaveChangesAsync();

        guidGeneratorMock.Verify(g => g.Generate(), Times.Never);
    }

    #endregion
}

/// <summary>
/// 用于 GUID 键测试的 DbContext
/// </summary>
public class GuidKeyDbContext : DbContext
{
    private readonly NoelleAutoSetGuidKeyInterceptor _interceptor;

    public DbSet<TestGuidKeyEntityWithNew> GuidKeyEntities { get; set; } = null!;

    public GuidKeyDbContext(DbContextOptions<GuidKeyDbContext> options, NoelleAutoSetGuidKeyInterceptor interceptor)
        : base(options)
    {
        _interceptor = interceptor;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_interceptor);
    }
}
