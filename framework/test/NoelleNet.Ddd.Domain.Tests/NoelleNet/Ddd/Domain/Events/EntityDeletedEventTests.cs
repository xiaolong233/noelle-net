using NoelleNet.Ddd.Domain.Entities;
using NoelleNet.Ddd.Domain.Events;

namespace NoelleNet.Ddd.Domain.Events;

public class EntityDeletedEventTests
{
    [Fact]
    public void Constructor_ShouldSetEntity()
    {
        var entity = new SimpleEventEntity { Id = 1 };
        var domainEvent = new EntityDeletedEvent<SimpleEventEntity>(entity);

        Assert.Equal(entity, domainEvent.Entity);
    }

    [Fact]
    public void ShouldImplementIDomainEvent()
    {
        var entity = new SimpleEventEntity { Id = 1 };
        var domainEvent = new EntityDeletedEvent<SimpleEventEntity>(entity);

        Assert.IsAssignableFrom<IDomainEvent>(domainEvent);
    }

    [Fact]
    public void ShouldBeRecord_ValueEquality()
    {
        var entity = new SimpleEventEntity { Id = 1 };
        var event1 = new EntityDeletedEvent<SimpleEventEntity>(entity);
        var event2 = new EntityDeletedEvent<SimpleEventEntity>(entity);

        Assert.Equal(event1, event2);
    }

    [Fact]
    public void ShouldBeRecord_DifferentEntities_ShouldNotBeEqual()
    {
        var entity1 = new SimpleEventEntity { Id = 1 };
        var entity2 = new SimpleEventEntity { Id = 2 };
        var event1 = new EntityDeletedEvent<SimpleEventEntity>(entity1);
        var event2 = new EntityDeletedEvent<SimpleEventEntity>(entity2);

        Assert.NotEqual(event1, event2);
    }
}

/// <summary>
/// 用于事件测试的简单实体
/// </summary>
internal class SimpleEventEntity : Entity
{
    public int Id { get; set; }

    public override object?[] GetIdentifiers() => [Id];
}
