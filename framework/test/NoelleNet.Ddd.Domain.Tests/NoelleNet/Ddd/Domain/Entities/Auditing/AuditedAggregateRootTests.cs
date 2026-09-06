using NoelleNet.Ddd.Domain.Events;

namespace NoelleNet.Ddd.Domain.Entities.Auditing;

/// <summary>
/// 审计聚合根基类（CreationAuditedAggregateRoot / AuditedAggregateRoot）的契约测试：
/// 审计字段与领域事件能力的组合继承
/// </summary>
public class AuditedAggregateRootTests
{
    /// <summary>
    /// 创建审计聚合根：默认值 + 领域事件能力继承
    /// </summary>
    [Fact]
    public void CreationAuditedAggregateRoot_ShouldHaveEmptyDefaultsAndDomainEvents()
    {
        var aggregate = new TestCreationAuditedAggregateRoot { Id = 1 };

        Assert.Equal(default, aggregate.CreatedAt);
        Assert.Null(aggregate.CreatedBy);
        Assert.Empty(aggregate.DomainEvents);
    }

    /// <summary>
    /// 审计聚合根：修改审计字段默认为 null
    /// </summary>
    [Fact]
    public void AuditedAggregateRoot_Defaults_ShouldBeNull()
    {
        var aggregate = new TestAuditedAggregateRoot { Id = 1 };

        Assert.Null(aggregate.LastModifiedAt);
        Assert.Null(aggregate.LastModifiedBy);
    }

    /// <summary>
    /// 泛型审计聚合根：Id 默认值 + 审计与事件能力继承
    /// </summary>
    [Fact]
    public void GenericAuditedAggregateRoot_ShouldInheritCapabilities()
    {
        var aggregate = new TestAuditedAggregateRootWithId();

        Assert.Equal(default, aggregate.Id);
        Assert.Null(aggregate.LastModifiedAt);
        Assert.Empty(aggregate.DomainEvents);
    }

    /// <summary>
    /// 创建审计聚合根
    /// </summary>
    internal class TestCreationAuditedAggregateRoot : CreationAuditedAggregateRoot
    {
        public int Id { get; set; }

        public override object?[] GetIdentifiers() => [Id];
    }

    /// <summary>
    /// 审计聚合根
    /// </summary>
    internal class TestAuditedAggregateRoot : AuditedAggregateRoot
    {
        public int Id { get; set; }

        public override object?[] GetIdentifiers() => [Id];
    }

    internal class TestAuditedAggregateRootWithId : AuditedAggregateRoot<Guid>
    {
    }
}
