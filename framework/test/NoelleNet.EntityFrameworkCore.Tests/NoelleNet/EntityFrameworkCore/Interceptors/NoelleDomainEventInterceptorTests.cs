using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using NoelleNet.Ddd.Domain.Events;
using NoelleNet.EventBus.Abstractions.Local;
using NoelleNet.EntityFrameworkCore.Interceptors;

namespace NoelleNet.EntityFrameworkCore.Interceptors;

#region Test Entities

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
/// 非 IHasDomainEvents 的实体（用于验证不会处理该类型实体）
/// </summary>
public class NonDomainEventEntity
{
    public int Id { get; set; }
}

#endregion

/// <summary>
/// <see cref="NoelleDomainEventInterceptor"/> 的单元测试
/// </summary>
public class NoelleDomainEventInterceptorTests
{
    private readonly Mock<ILocalEventBus> _localEventBusMock;
    private readonly NoelleDomainEventInterceptor _interceptor;

    public NoelleDomainEventInterceptorTests()
    {
        _localEventBusMock = new Mock<ILocalEventBus>();
        _interceptor = new NoelleDomainEventInterceptor(_localEventBusMock.Object);
    }

    #region 构造函数

    [Fact]
    public void Constructor_LocalEventBusIsNull_ShouldThrowArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new NoelleDomainEventInterceptor(null!));
        Assert.Equal("localEventBus", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidEventBus_ShouldCreateInstance()
    {
        var bus = new Mock<ILocalEventBus>().Object;
        var interceptor = new NoelleDomainEventInterceptor(bus);

        Assert.NotNull(interceptor);
    }

    #endregion

    #region SavingChanges

    [Fact]
    public void SavingChanges_EntitiesWithDomainEvents_ShouldPublishAllEvents()
    {
        var options = new DbContextOptionsBuilder<DomainEventDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new DomainEventDbContext(options, _interceptor);

        var entity1 = new TestDomainEventEntity { Id = 1 };
        entity1.AddDomainEvent(new TestDomainEvent { Data = "event1" });
        entity1.AddDomainEvent(new AnotherTestDomainEvent { Value = 42 });

        var entity2 = new TestDomainEventEntity { Id = 2 };
        entity2.AddDomainEvent(new TestDomainEvent { Data = "event2" });

        context.DomainEventEntities.Add(entity1);
        context.DomainEventEntities.Add(entity2);

        context.SaveChanges();

        _localEventBusMock.Verify(bus => bus.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public void SavingChanges_ShouldClearDomainEventsAfterDispatch()
    {
        var options = new DbContextOptionsBuilder<DomainEventDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new DomainEventDbContext(options, _interceptor);

        var entity = new TestDomainEventEntity { Id = 1 };
        entity.AddDomainEvent(new TestDomainEvent { Data = "test" });
        context.DomainEventEntities.Add(entity);

        context.SaveChanges();

        Assert.Empty(entity.DomainEvents);
    }

    [Fact]
    public void SavingChanges_NoDomainEventEntities_ShouldNotPublishEvents()
    {
        var options = new DbContextOptionsBuilder<DomainEventDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new DomainEventDbContext(options, _interceptor);

        context.NonDomainEventEntities.Add(new NonDomainEventEntity { Id = 1 });

        context.SaveChanges();

        _localEventBusMock.Verify(bus => bus.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void SavingChanges_EntityHasEmptyDomainEvents_ShouldNotPublish()
    {
        var options = new DbContextOptionsBuilder<DomainEventDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new DomainEventDbContext(options, _interceptor);

        var entity = new TestDomainEventEntity { Id = 1 };
        context.DomainEventEntities.Add(entity);

        context.SaveChanges();

        _localEventBusMock.Verify(bus => bus.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void SavingChanges_MultipleEntityTypes_ShouldOnlyHandleDomainEventEntities()
    {
        var options = new DbContextOptionsBuilder<DomainEventDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new DomainEventDbContext(options, _interceptor);

        var domainEntity = new TestDomainEventEntity { Id = 1 };
        domainEntity.AddDomainEvent(new TestDomainEvent { Data = "valid" });

        context.DomainEventEntities.Add(domainEntity);
        context.NonDomainEventEntities.Add(new NonDomainEventEntity { Id = 99 });

        context.SaveChanges();

        _localEventBusMock.Verify(bus => bus.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region SavingChangesAsync

    [Fact]
    public async Task SavingChangesAsync_EntitiesWithDomainEvents_ShouldPublishAllEvents()
    {
        var options = new DbContextOptionsBuilder<DomainEventDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new DomainEventDbContext(options, _interceptor);

        var entity = new TestDomainEventEntity { Id = 1 };
        entity.AddDomainEvent(new TestDomainEvent { Data = "async-event" });
        entity.AddDomainEvent(new AnotherTestDomainEvent { Value = 100 });
        context.DomainEventEntities.Add(entity);

        await context.SaveChangesAsync();

        _localEventBusMock.Verify(bus => bus.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task SavingChangesAsync_ShouldClearDomainEventsAfterDispatch()
    {
        var options = new DbContextOptionsBuilder<DomainEventDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new DomainEventDbContext(options, _interceptor);

        var entity = new TestDomainEventEntity { Id = 1 };
        entity.AddDomainEvent(new TestDomainEvent { Data = "async-clear" });
        context.DomainEventEntities.Add(entity);

        await context.SaveChangesAsync();

        Assert.Empty(entity.DomainEvents);
    }

    [Fact]
    public async Task SavingChangesAsync_NoDomainEvents_ShouldNotPublish()
    {
        var options = new DbContextOptionsBuilder<DomainEventDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new DomainEventDbContext(options, _interceptor);

        context.NonDomainEventEntities.Add(new NonDomainEventEntity { Id = 1 });

        await context.SaveChangesAsync();

        _localEventBusMock.Verify(bus => bus.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}

/// <summary>
/// 用于领域事件测试的 DbContext
/// </summary>
public class DomainEventDbContext : DbContext
{
    private readonly NoelleDomainEventInterceptor _interceptor;

    public DbSet<TestDomainEventEntity> DomainEventEntities { get; set; } = null!;
    public DbSet<NonDomainEventEntity> NonDomainEventEntities { get; set; } = null!;

    public DomainEventDbContext(DbContextOptions<DomainEventDbContext> options, NoelleDomainEventInterceptor interceptor)
        : base(options)
    {
        _interceptor = interceptor;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_interceptor);
    }
}
