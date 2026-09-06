using Microsoft.EntityFrameworkCore;
using Moq;
using NoelleNet.Ddd.Domain.Entities;
using NoelleNet.EntityFrameworkCore.Interceptors;

namespace NoelleNet.EntityFrameworkCore.Interceptors;

/// <summary>
/// <see cref="NoelleAutoSetGuidKeyInterceptor"/> 的行为测试。
/// 注意：EF Core 在 DetectChanges 阶段即通过客户端生成器为 Guid 键赋值，
/// 因此默认配置下拦截器观察到的 Id 恒非空、不会触发；
/// 拦截器的真实生效场景是键关闭了值生成（ValueGeneratedNever）的实体。
/// 键最终值的归属取决于 EF 值生成与拦截器的先后（EF 内部实现细节），
/// 因此仅锁定拦截器自身逻辑：空 Id 调用生成器、非空 Id 跳过且不覆盖。
/// </summary>
public class NoelleAutoSetGuidKeyInterceptorTests
{
    /// <summary>
    /// 关闭键值生成（ValueGeneratedNever）且 Id 为空时，拦截器应调用生成器一次
    /// </summary>
    [Fact]
    public async Task SavingChangesAsync_WhenKeyGenerationDisabledAndIdEmpty_ShouldCallGenerator()
    {
        var guidGeneratorMock = new Mock<IGuidGenerator>();
        guidGeneratorMock.Setup(g => g.Generate()).Returns(Guid.NewGuid());
        var context = CreateContext(guidGeneratorMock.Object, valueGeneratedNever: true);

        var entity = new GuidKeyTestEntity { Name = "test" };
        context.GuidKeyEntities.Add(entity);

        await context.SaveChangesAsync();

        guidGeneratorMock.Verify(g => g.Generate(), Times.Once);
    }

    /// <summary>
    /// Id 非空时（构造函数已生成）不应调用生成器、不覆盖原值（同步路径代表）
    /// </summary>
    [Fact]
    public void SavingChanges_EntityWithExistingId_ShouldNotCallGenerator()
    {
        var guidGeneratorMock = new Mock<IGuidGenerator>();
        var existingId = Guid.NewGuid();
        var context = CreateContext(guidGeneratorMock.Object, valueGeneratedNever: true);

        var entity = new GuidKeyTestEntity(existingId) { Name = "test" };
        context.GuidKeyEntities.Add(entity);

        context.SaveChanges();

        Assert.Equal(existingId, entity.Id);
        guidGeneratorMock.Verify(g => g.Generate(), Times.Never);
    }

    private static GuidKeyDbContext CreateContext(IGuidGenerator generator, bool valueGeneratedNever)
    {
        var options = new DbContextOptionsBuilder<GuidKeyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new GuidKeyDbContext(options, new NoelleAutoSetGuidKeyInterceptor(generator), valueGeneratedNever);
    }
}

/// <summary>
/// 真实形态的 Guid 键测试实体（protected Id setter，无 new 隐藏）
/// </summary>
public class GuidKeyTestEntity : Entity<Guid>
{
    public GuidKeyTestEntity() { }

    public GuidKeyTestEntity(Guid id) : base(id) { }

    public string Name { get; set; } = "";
}

/// <summary>
/// GUID 键测试 DbContext
/// </summary>
public class GuidKeyDbContext : DbContext
{
    private readonly NoelleAutoSetGuidKeyInterceptor _interceptor;
    private readonly bool _valueGeneratedNever;

    public GuidKeyDbContext(DbContextOptions<GuidKeyDbContext> options, NoelleAutoSetGuidKeyInterceptor interceptor, bool valueGeneratedNever)
        : base(options)
    {
        _interceptor = interceptor;
        _valueGeneratedNever = valueGeneratedNever;
    }

    public DbSet<GuidKeyTestEntity> GuidKeyEntities { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (_valueGeneratedNever)
            modelBuilder.Entity<GuidKeyTestEntity>().Property(x => x.Id).ValueGeneratedNever();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_interceptor);
    }
}
