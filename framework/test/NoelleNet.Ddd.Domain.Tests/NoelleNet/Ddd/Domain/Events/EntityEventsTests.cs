using NoelleNet.Ddd.Domain.Entities;

namespace NoelleNet.Ddd.Domain.Events;

/// <summary>
/// 领域事件类型的契约测试：事件载荷与 EntityChangeType 枚举值契约
/// （demo 等使用方依赖 Create/Update/Delete 的语义）
/// </summary>
public class EntityEventsTests
{
    private static TestEventEntity CreateEntity(int id = 1) => new() { Id = id };

    /// <summary>
    /// EntityCreatedEvent / EntityUpdatedEvent / EntityDeletedEvent 应携带实体载荷
    /// </summary>
    [Fact]
    public void EntityEvents_ShouldCarryEntity()
    {
        var entity = CreateEntity();

        Assert.Equal(entity, new EntityCreatedEvent<TestEventEntity>(entity).Entity);
        Assert.Equal(entity, new EntityUpdatedEvent<TestEventEntity>(entity).Entity);
        Assert.Equal(entity, new EntityDeletedEvent<TestEventEntity>(entity).Entity);
    }

    /// <summary>
    /// EntityChangedEvent 应携带实体与变更类型
    /// </summary>
    [Fact]
    public void EntityChangedEvent_ShouldCarryEntityAndChangeType()
    {
        var entity = CreateEntity();

        var domainEvent = new EntityChangedEvent<TestEventEntity>(entity, EntityChangeType.Update);

        Assert.Equal(entity, domainEvent.Entity);
        Assert.Equal(EntityChangeType.Update, domainEvent.ChangeType);
    }

    /// <summary>
    /// EntityChangeType 枚举值契约：Create=0、Update=1、Delete=2
    /// </summary>
    [Fact]
    public void EntityChangeType_ShouldHaveStableValues()
    {
        Assert.Equal(0, (int)EntityChangeType.Create);
        Assert.Equal(1, (int)EntityChangeType.Update);
        Assert.Equal(2, (int)EntityChangeType.Delete);
    }
}

/// <summary>
/// 事件测试实体
/// </summary>
internal class TestEventEntity : Entity
{
    public int Id { get; set; }

    public override object?[] GetIdentifiers() => [Id];
}
