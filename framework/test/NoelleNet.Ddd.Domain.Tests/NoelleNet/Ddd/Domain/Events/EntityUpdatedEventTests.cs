using NoelleNet.Ddd.Domain.Entities;
using NoelleNet.Ddd.Domain.Events;

namespace NoelleNet.Ddd.Domain.Events;

/// <summary>
/// 用于事件测试的简单实体
/// </summary>
internal class EventEntity : Entity
{
    public int Id { get; set; }

    public override object?[] GetIdentifiers() => [Id];
}

public class EntityUpdatedEventTests
{
    [Fact]
    public void Constructor_ShouldSetEntity()
    {
        var entity = new EventEntity { Id = 1 };
        var domainEvent = new EntityUpdatedEvent<EventEntity>(entity);

        Assert.Equal(entity, domainEvent.Entity);
    }

    [Fact]
    public void ShouldImplementIDomainEvent()
    {
        var entity = new EventEntity { Id = 1 };
        var domainEvent = new EntityUpdatedEvent<EventEntity>(entity);

        Assert.IsAssignableFrom<IDomainEvent>(domainEvent);
    }

    [Fact]
    public void ShouldBeRecord_ValueEquality()
    {
        var entity = new EventEntity { Id = 1 };
        var event1 = new EntityUpdatedEvent<EventEntity>(entity);
        var event2 = new EntityUpdatedEvent<EventEntity>(entity);

        Assert.Equal(event1, event2);
    }

    [Fact]
    public void ShouldBeRecord_DifferentEntities_ShouldNotBeEqual()
    {
        var entity1 = new EventEntity { Id = 1 };
        var entity2 = new EventEntity { Id = 2 };
        var event1 = new EntityUpdatedEvent<EventEntity>(entity1);
        var event2 = new EntityUpdatedEvent<EventEntity>(entity2);

        Assert.NotEqual(event1, event2);
    }
}
