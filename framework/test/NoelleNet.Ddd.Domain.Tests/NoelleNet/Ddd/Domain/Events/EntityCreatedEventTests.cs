using NoelleNet.Ddd.Domain.Entities;
using NoelleNet.Ddd.Domain.Events;

namespace NoelleNet.Ddd.Domain.Events;

public class EntityCreatedEventTests
{
    [Fact]
    public void Constructor_ShouldSetEntity()
    {
        var entity = new SimpleEntity { Id = 1 };
        var domainEvent = new EntityCreatedEvent<SimpleEntity>(entity);

        Assert.Equal(entity, domainEvent.Entity);
    }

    [Fact]
    public void ShouldImplementIDomainEvent()
    {
        var entity = new SimpleEntity { Id = 1 };
        var domainEvent = new EntityCreatedEvent<SimpleEntity>(entity);

        Assert.IsAssignableFrom<IDomainEvent>(domainEvent);
    }

    [Fact]
    public void ShouldBeRecord_ValueEquality()
    {
        var entity = new SimpleEntity { Id = 1 };
        var event1 = new EntityCreatedEvent<SimpleEntity>(entity);
        var event2 = new EntityCreatedEvent<SimpleEntity>(entity);

        Assert.Equal(event1, event2);
        Assert.True(event1 == event2);
    }

    [Fact]
    public void ShouldBeRecord_DifferentEntities_ShouldNotBeEqual()
    {
        var entity1 = new SimpleEntity { Id = 1 };
        var entity2 = new SimpleEntity { Id = 2 };
        var event1 = new EntityCreatedEvent<SimpleEntity>(entity1);
        var event2 = new EntityCreatedEvent<SimpleEntity>(entity2);

        Assert.NotEqual(event1, event2);
    }

    [Fact]
    public void SupportsDifferentEntityTypes()
    {
        var event1 = new EntityCreatedEvent<SimpleEntity>(new SimpleEntity { Id = 1 });
        var event2 = new EntityCreatedEvent<SimpleEntity>(new SimpleEntity { Id = 1 });

        Assert.Equal(event1, event2);
    }
}

/// <summary>
/// 用于事件测试的简单实体
/// </summary>
internal class SimpleEntity : Entity
{
    public int Id { get; set; }

    public override object?[] GetIdentifiers() => [Id];
}
