using NoelleNet.Ddd.Domain.Entities;
using NoelleNet.Ddd.Domain.Events;

namespace NoelleNet.Ddd.Domain.Entities;

/// <summary>
/// 用于测试的聚合根实现（无类型标识符）
/// </summary>
internal class TestAggregateRoot : AggregateRoot
{
    public int Id { get; set; }

    public override object?[] GetIdentifiers() => [Id];

    public void TestAddDomainEvent(IDomainEvent eventData) => AddDomainEvent(eventData);

    public void TestRemoveDomainEvent(IDomainEvent eventData) => RemoveDomainEvent(eventData);
}

/// <summary>
/// 用于测试的聚合根实现（带类型标识符）
/// </summary>
internal class TestAggregateRootWithId : AggregateRoot<Guid>
{
    public TestAggregateRootWithId() { }

    public TestAggregateRootWithId(Guid id) : base(id) { }

    public void TestAddDomainEvent(IDomainEvent eventData) => AddDomainEvent(eventData);

    public void TestRemoveDomainEvent(IDomainEvent eventData) => RemoveDomainEvent(eventData);
}

/// <summary>
/// 用于测试的实体（实现 IEntity 但不实现 IHasDomainEvents）
/// </summary>
internal class SimpleEntity : Entity
{
    public int Id { get; set; }

    public override object?[] GetIdentifiers() => [Id];
}

public class AggregateRootTests
{
    #region AggregateRoot (无类型参数)

    [Fact]
    public void ShouldImplementIAggregateRoot()
    {
        var aggregate = new TestAggregateRoot { Id = 1 };

        Assert.IsAssignableFrom<IAggregateRoot>(aggregate);
    }

    [Fact]
    public void ShouldImplementIHasDomainEvents()
    {
        var aggregate = new TestAggregateRoot { Id = 1 };

        Assert.IsAssignableFrom<IHasDomainEvents>(aggregate);
    }

    [Fact]
    public void ShouldImplementIEntity()
    {
        var aggregate = new TestAggregateRoot { Id = 1 };

        Assert.IsAssignableFrom<IEntity>(aggregate);
    }

    [Fact]
    public void AddDomainEvent_DomainEvents_ShouldContainAddedEvent()
    {
        var aggregate = new TestAggregateRoot { Id = 1 };
        var entity = new SimpleEntity { Id = 1 };
        var domainEvent = new EntityCreatedEvent<SimpleEntity>(entity);

        aggregate.TestAddDomainEvent(domainEvent);

        Assert.Single(aggregate.DomainEvents);
        Assert.Contains(domainEvent, aggregate.DomainEvents);
    }

    [Fact]
    public void AddDomainEvent_MultipleEvents_ShouldMaintainOrder()
    {
        var aggregate = new TestAggregateRoot { Id = 1 };
        var entity = new SimpleEntity { Id = 1 };
        var event1 = new EntityCreatedEvent<SimpleEntity>(entity);
        var event2 = new EntityUpdatedEvent<SimpleEntity>(entity);

        aggregate.TestAddDomainEvent(event1);
        aggregate.TestAddDomainEvent(event2);

        Assert.Equal(2, aggregate.DomainEvents.Count);
        Assert.Equal(event1, aggregate.DomainEvents.First());
        Assert.Equal(event2, aggregate.DomainEvents.Last());
    }

    [Fact]
    public void RemoveDomainEvent_ExistingEvent_ShouldRemoveIt()
    {
        var aggregate = new TestAggregateRoot { Id = 1 };
        var entity = new SimpleEntity { Id = 1 };
        var event1 = new EntityCreatedEvent<SimpleEntity>(entity);
        var event2 = new EntityUpdatedEvent<SimpleEntity>(entity);
        aggregate.TestAddDomainEvent(event1);
        aggregate.TestAddDomainEvent(event2);

        aggregate.TestRemoveDomainEvent(event1);

        Assert.Single(aggregate.DomainEvents);
        Assert.Contains(event2, aggregate.DomainEvents);
        Assert.DoesNotContain(event1, aggregate.DomainEvents);
    }

    [Fact]
    public void RemoveDomainEvent_NonExistingEvent_ShouldDoNothing()
    {
        var aggregate = new TestAggregateRoot { Id = 1 };
        var entity = new SimpleEntity { Id = 1 };
        var event1 = new EntityCreatedEvent<SimpleEntity>(entity);
        var event2 = new EntityUpdatedEvent<SimpleEntity>(entity);
        aggregate.TestAddDomainEvent(event1);

        aggregate.TestRemoveDomainEvent(event2);

        Assert.Single(aggregate.DomainEvents);
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        var aggregate = new TestAggregateRoot { Id = 1 };
        var entity = new SimpleEntity { Id = 1 };
        aggregate.TestAddDomainEvent(new EntityCreatedEvent<SimpleEntity>(entity));
        aggregate.TestAddDomainEvent(new EntityUpdatedEvent<SimpleEntity>(entity));

        aggregate.ClearDomainEvents();

        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void ClearDomainEvents_WhenEmpty_ShouldNotThrow()
    {
        var aggregate = new TestAggregateRoot { Id = 1 };

        aggregate.ClearDomainEvents();

        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void DomainEvents_ShouldBeReadOnly()
    {
        var aggregate = new TestAggregateRoot { Id = 1 };

        Assert.IsAssignableFrom<IReadOnlyCollection<IDomainEvent>>(aggregate.DomainEvents);
    }

    [Fact]
    public void DomainEvents_InitialState_ShouldBeEmpty()
    {
        var aggregate = new TestAggregateRoot { Id = 1 };

        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void Equals_SameId_ShouldReturnTrue()
    {
        var aggregate1 = new TestAggregateRoot { Id = 1 };
        var aggregate2 = new TestAggregateRoot { Id = 1 };

        Assert.True(aggregate1.Equals(aggregate2));
    }

    #endregion

    #region AggregateRoot<TIdentifier>

    [Fact]
    public void GenericAggregateRoot_DefaultConstructor_IdShouldBeDefault()
    {
        var aggregate = new TestAggregateRootWithId();

        Assert.Equal(default, aggregate.Id);
    }

    [Fact]
    public void GenericAggregateRoot_ConstructorWithId_ShouldSetId()
    {
        var guid = Guid.NewGuid();
        var aggregate = new TestAggregateRootWithId(guid);

        Assert.Equal(guid, aggregate.Id);
    }

    [Fact]
    public void GenericAggregateRoot_ShouldImplementIAggregateRoot()
    {
        var aggregate = new TestAggregateRootWithId(Guid.NewGuid());

        Assert.IsAssignableFrom<IAggregateRoot>(aggregate);
    }

    [Fact]
    public void GenericAggregateRoot_ShouldImplementIHasDomainEvents()
    {
        var aggregate = new TestAggregateRootWithId(Guid.NewGuid());

        Assert.IsAssignableFrom<IHasDomainEvents>(aggregate);
    }

    [Fact]
    public void GenericAggregateRoot_ShouldImplementIEntityOfT()
    {
        var aggregate = new TestAggregateRootWithId(Guid.NewGuid());

        Assert.IsAssignableFrom<IEntity<Guid>>(aggregate);
    }

    [Fact]
    public void GenericAggregateRoot_AddAndClearDomainEvents()
    {
        var aggregate = new TestAggregateRootWithId(Guid.NewGuid());
        var entity = new SimpleEntity { Id = 1 };
        aggregate.TestAddDomainEvent(new EntityDeletedEvent<SimpleEntity>(entity));
        aggregate.TestAddDomainEvent(new EntityCreatedEvent<SimpleEntity>(entity));

        Assert.Equal(2, aggregate.DomainEvents.Count);

        aggregate.ClearDomainEvents();

        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void GenericAggregateRoot_RemoveThenAddDomainEvents()
    {
        var aggregate = new TestAggregateRootWithId(Guid.NewGuid());
        var entity = new SimpleEntity { Id = 1 };
        var deletedEvent = new EntityDeletedEvent<SimpleEntity>(entity);
        var createdEvent = new EntityCreatedEvent<SimpleEntity>(entity);

        aggregate.TestAddDomainEvent(deletedEvent);
        aggregate.TestAddDomainEvent(createdEvent);
        aggregate.TestRemoveDomainEvent(deletedEvent);

        Assert.Single(aggregate.DomainEvents);
        Assert.Equal(createdEvent, aggregate.DomainEvents.Single());
    }

    #endregion
}
