using NoelleNet.Ddd.Domain.Events;

namespace NoelleNet.Ddd.Domain.Entities;

/// <summary>
/// 聚合根测试实体
/// </summary>
internal class TestAggregateRoot : AggregateRoot
{
    public int Id { get; set; }

    public override object?[] GetIdentifiers() => [Id];

    public void TestAddDomainEvent(IDomainEvent eventData) => AddDomainEvent(eventData);

    public void TestRemoveDomainEvent(IDomainEvent eventData) => RemoveDomainEvent(eventData);
}

internal class TestAggregateRootWithId : AggregateRoot<Guid>
{
    public TestAggregateRootWithId() { }

    public TestAggregateRootWithId(Guid id) : base(id) { }

    public void TestAddDomainEvent(IDomainEvent eventData) => AddDomainEvent(eventData);
}

/// <summary>
/// <see cref="AggregateRoot"/> 的契约测试：领域事件的挂载、顺序、移除与清空
/// </summary>
public class AggregateRootTests
{
    private static EntityCreatedEvent<TestAggregateRoot> CreateEvent(TestAggregateRoot root)
        => new(root);

    /// <summary>
    /// 初始状态事件集合为空
    /// </summary>
    [Fact]
    public void DomainEvents_InitialState_ShouldBeEmpty()
    {
        var aggregate = new TestAggregateRoot { Id = 1 };

        Assert.Empty(aggregate.DomainEvents);
    }

    /// <summary>
    /// 挂载事件应保持添加顺序；移除指定事件不影响其他事件；清空移除全部
    /// </summary>
    [Fact]
    public void DomainEvents_AddRemoveAndClear_ShouldMaintainOrder()
    {
        var aggregate = new TestAggregateRoot { Id = 1 };
        var event1 = new EntityCreatedEvent<TestAggregateRoot>(aggregate);
        var event2 = new EntityUpdatedEvent<TestAggregateRoot>(aggregate);

        aggregate.TestAddDomainEvent(event1);
        aggregate.TestAddDomainEvent(event2);

        Assert.Equal([event1, event2], aggregate.DomainEvents);

        aggregate.TestRemoveDomainEvent(event1);
        Assert.Equal([event2], aggregate.DomainEvents);

        aggregate.TestRemoveDomainEvent(event1); // 移除不存在的事件不应抛异常
        Assert.Single(aggregate.DomainEvents);

        aggregate.ClearDomainEvents();
        Assert.Empty(aggregate.DomainEvents);

        aggregate.ClearDomainEvents(); // 空集合清空不应抛异常
        Assert.Empty(aggregate.DomainEvents);
    }

    /// <summary>
    /// 泛型聚合根：构造函数设置 Id，事件能力继承自基类
    /// </summary>
    [Fact]
    public void GenericAggregateRoot_ShouldSetIdAndSupportDomainEvents()
    {
        var guid = Guid.NewGuid();
        var aggregate = new TestAggregateRootWithId(guid);

        Assert.Equal(guid, aggregate.Id);

        aggregate.TestAddDomainEvent(new EntityDeletedEvent<TestAggregateRootWithId>(aggregate));
        Assert.Single(aggregate.DomainEvents);
    }
}
