using NoelleNet.Ddd.Domain.Entities;
using NoelleNet.Ddd.Domain.Events;

namespace NoelleNet.Ddd.Domain.Events;

public class EntityChangedEventTests
{
    [Fact]
    public void Constructor_ShouldSetEntityAndChangeType()
    {
        var entity = new ChangeEventEntity { Id = 1 };
        var domainEvent = new EntityChangedEvent<ChangeEventEntity>(entity, EntityChangeType.Create);

        Assert.Equal(entity, domainEvent.Entity);
        Assert.Equal(EntityChangeType.Create, domainEvent.ChangeType);
    }

    [Fact]
    public void ShouldImplementIDomainEvent()
    {
        var entity = new ChangeEventEntity { Id = 1 };
        var domainEvent = new EntityChangedEvent<ChangeEventEntity>(entity, EntityChangeType.Update);

        Assert.IsAssignableFrom<IDomainEvent>(domainEvent);
    }

    [Fact]
    public void ShouldBeRecord_ValueEquality_SameData()
    {
        var entity = new ChangeEventEntity { Id = 1 };
        var event1 = new EntityChangedEvent<ChangeEventEntity>(entity, EntityChangeType.Delete);
        var event2 = new EntityChangedEvent<ChangeEventEntity>(entity, EntityChangeType.Delete);

        Assert.Equal(event1, event2);
    }

    [Fact]
    public void ShouldBeRecord_DifferentEntity_ShouldNotBeEqual()
    {
        var entity1 = new ChangeEventEntity { Id = 1 };
        var entity2 = new ChangeEventEntity { Id = 2 };
        var event1 = new EntityChangedEvent<ChangeEventEntity>(entity1, EntityChangeType.Create);
        var event2 = new EntityChangedEvent<ChangeEventEntity>(entity2, EntityChangeType.Create);

        Assert.NotEqual(event1, event2);
    }

    [Fact]
    public void ShouldBeRecord_DifferentChangeType_ShouldNotBeEqual()
    {
        var entity = new ChangeEventEntity { Id = 1 };
        var event1 = new EntityChangedEvent<ChangeEventEntity>(entity, EntityChangeType.Create);
        var event2 = new EntityChangedEvent<ChangeEventEntity>(entity, EntityChangeType.Update);

        Assert.NotEqual(event1, event2);
    }

    [Fact]
    public void EntityChangeType_ShouldHaveExpectedValues()
    {
        Assert.Equal(0, (int)EntityChangeType.Create);
        Assert.Equal(1, (int)EntityChangeType.Update);
        Assert.Equal(2, (int)EntityChangeType.Delete);
    }

    [Fact]
    public void SupportsAllChangeTypes()
    {
        var entity = new ChangeEventEntity { Id = 1 };

        var createEvent = new EntityChangedEvent<ChangeEventEntity>(entity, EntityChangeType.Create);
        var updateEvent = new EntityChangedEvent<ChangeEventEntity>(entity, EntityChangeType.Update);
        var deleteEvent = new EntityChangedEvent<ChangeEventEntity>(entity, EntityChangeType.Delete);

        Assert.IsAssignableFrom<IDomainEvent>(createEvent);
        Assert.IsAssignableFrom<IDomainEvent>(updateEvent);
        Assert.IsAssignableFrom<IDomainEvent>(deleteEvent);
    }
}

/// <summary>
/// 用于事件测试的简单实体
/// </summary>
internal class ChangeEventEntity : Entity
{
    public int Id { get; set; }

    public override object?[] GetIdentifiers() => [Id];
}
