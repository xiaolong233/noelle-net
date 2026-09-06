using Microsoft.EntityFrameworkCore;
using Moq;
using NoelleNet.Ddd.Domain.Events;
using NoelleNet.EventBus.Abstractions.Local;
using NoelleNet.EntityFrameworkCore.Interceptors;

namespace NoelleNet.EntityFrameworkCore.Interceptors;

/// <summary>
/// <see cref="NoelleDomainEventInterceptor"/> 的行为测试：提交前派发、派发后清空、仅处理含事件的实体
/// </summary>
public class NoelleDomainEventInterceptorTests
{
    /// <summary>
    /// 保存含事件的实体时应发布全部事件（保持添加顺序）
    /// </summary>
    [Fact]
    public async Task SavingChangesAsync_EntitiesWithDomainEvents_ShouldPublishAllEvents()
    {
        var eventBusMock = new Mock<ILocalEventBus>();
        var context = CreateContext(new NoelleDomainEventInterceptor(eventBusMock.Object));

        var entity = new TestDomainEventEntity { Id = 1 };
        entity.AddDomainEvent(new TestDomainEvent { Data = "event1" });
        entity.AddDomainEvent(new AnotherTestDomainEvent { Value = 42 });
        context.DomainEventEntities.Add(entity);

        await context.SaveChangesAsync();

        eventBusMock.Verify(bus => bus.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    /// <summary>
    /// 派发后应清空实体上的领域事件（防止重复派发）
    /// </summary>
    [Fact]
    public async Task SavingChangesAsync_ShouldClearDomainEventsAfterDispatch()
    {
        var context = CreateContext(new NoelleDomainEventInterceptor(new Mock<ILocalEventBus>().Object));

        var entity = new TestDomainEventEntity { Id = 1 };
        entity.AddDomainEvent(new TestDomainEvent { Data = "async-clear" });
        context.DomainEventEntities.Add(entity);

        await context.SaveChangesAsync();

        Assert.Empty(entity.DomainEvents);
    }

    /// <summary>
    /// 无事件或只有非事件实体时不应发布（同步路径代表）
    /// </summary>
    [Fact]
    public void SavingChanges_WithoutDomainEvents_ShouldNotPublish()
    {
        var eventBusMock = new Mock<ILocalEventBus>();
        var context = CreateContext(new NoelleDomainEventInterceptor(eventBusMock.Object));

        context.DomainEventEntities.Add(new TestDomainEventEntity { Id = 1 });   // 无事件
        context.NonDomainEventEntities.Add(new NonDomainEventEntity { Id = 99 }); // 非事件实体

        context.SaveChanges();

        eventBusMock.Verify(bus => bus.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static DomainEventDbContext CreateContext(NoelleDomainEventInterceptor interceptor)
    {
        var options = new DbContextOptionsBuilder<DomainEventDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new DomainEventDbContext(options, interceptor);
    }
}

/// <summary>
/// 测试用的领域事件
/// </summary>
public class TestDomainEvent : IDomainEvent
{
    public string Data { get; set; } = "";
}

/// <summary>
/// 测试用的领域事件（另一种类型）
/// </summary>
public class AnotherTestDomainEvent : IDomainEvent
{
    public int Value { get; set; }
}

/// <summary>
/// 含领域事件的测试实体
/// </summary>
public class TestDomainEventEntity : IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public int Id { get; set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}

/// <summary>
/// 非 IHasDomainEvents 的实体
/// </summary>
public class NonDomainEventEntity
{
    public int Id { get; set; }
}

/// <summary>
/// 领域事件测试 DbContext
/// </summary>
public class DomainEventDbContext : DbContext
{
    private readonly NoelleDomainEventInterceptor _interceptor;

    public DomainEventDbContext(DbContextOptions<DomainEventDbContext> options, NoelleDomainEventInterceptor interceptor)
        : base(options)
    {
        _interceptor = interceptor;
    }

    public DbSet<TestDomainEventEntity> DomainEventEntities { get; set; } = null!;
    public DbSet<NonDomainEventEntity> NonDomainEventEntities { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_interceptor);
    }
}
