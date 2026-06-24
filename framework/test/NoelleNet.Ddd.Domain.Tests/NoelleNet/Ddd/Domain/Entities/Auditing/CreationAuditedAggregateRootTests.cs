using NoelleNet.Auditing;
using NoelleNet.Ddd.Domain.Entities.Auditing;
using NoelleNet.Ddd.Domain.Events;

namespace NoelleNet.Ddd.Domain.Entities.Auditing;

/// <summary>
/// 用于测试的创建审计聚合根实现
/// </summary>
internal class TestCreationAuditedAggregateRoot : CreationAuditedAggregateRoot
{
    public int Id { get; set; }

    public override object?[] GetIdentifiers() => [Id];
}

internal class TestCreationAuditedAggregateRootWithId : CreationAuditedAggregateRoot<Guid>
{
    public TestCreationAuditedAggregateRootWithId() { }
}

public class CreationAuditedAggregateRootTests
{
    #region CreationAuditedAggregateRoot (无类型参数)

    [Fact]
    public void ShouldImplementICreationAudited()
    {
        var aggregate = new TestCreationAuditedAggregateRoot { Id = 1 };

        Assert.IsAssignableFrom<ICreationAudited>(aggregate);
    }

    [Fact]
    public void ShouldImplementIAggregateRoot()
    {
        var aggregate = new TestCreationAuditedAggregateRoot { Id = 1 };

        Assert.IsAssignableFrom<IAggregateRoot>(aggregate);
    }

    [Fact]
    public void ShouldImplementIEntity()
    {
        var aggregate = new TestCreationAuditedAggregateRoot { Id = 1 };

        Assert.IsAssignableFrom<IEntity>(aggregate);
    }

    [Fact]
    public void ShouldImplementIHasDomainEvents()
    {
        var aggregate = new TestCreationAuditedAggregateRoot { Id = 1 };

        Assert.IsAssignableFrom<IHasDomainEvents>(aggregate);
    }

    [Fact]
    public void CreatedAt_DefaultValue_ShouldBeDefaultDateTime()
    {
        var aggregate = new TestCreationAuditedAggregateRoot { Id = 1 };

        Assert.Equal(default, aggregate.CreatedAt);
    }

    [Fact]
    public void CreatedBy_DefaultValue_ShouldBeNull()
    {
        var aggregate = new TestCreationAuditedAggregateRoot { Id = 1 };

        Assert.Null(aggregate.CreatedBy);
    }

    [Fact]
    public void DomainEvents_ShouldBeInheritedFromAggregateRoot()
    {
        var aggregate = new TestCreationAuditedAggregateRoot { Id = 1 };

        Assert.Empty(aggregate.DomainEvents);
    }

    #endregion

    #region CreationAuditedAggregateRoot<TIdentifier>

    [Fact]
    public void GenericCreationAuditedAggregateRoot_ShouldImplementICreationAudited()
    {
        var aggregate = new TestCreationAuditedAggregateRootWithId();

        Assert.IsAssignableFrom<ICreationAudited>(aggregate);
    }

    [Fact]
    public void GenericCreationAuditedAggregateRoot_ShouldImplementIAggregateRoot()
    {
        var aggregate = new TestCreationAuditedAggregateRootWithId();

        Assert.IsAssignableFrom<IAggregateRoot>(aggregate);
    }

    [Fact]
    public void GenericCreationAuditedAggregateRoot_ShouldImplementIEntityOfT()
    {
        var aggregate = new TestCreationAuditedAggregateRootWithId();

        Assert.IsAssignableFrom<IEntity<Guid>>(aggregate);
    }

    [Fact]
    public void GenericCreationAuditedAggregateRoot_IdDefaultValue_ShouldBeDefault()
    {
        var aggregate = new TestCreationAuditedAggregateRootWithId();

        Assert.Equal(default, aggregate.Id);
    }

    [Fact]
    public void GenericCreationAuditedAggregateRoot_CreatedAt_DefaultValue_ShouldBeDefaultDateTime()
    {
        var aggregate = new TestCreationAuditedAggregateRootWithId();

        Assert.Equal(default, aggregate.CreatedAt);
    }

    #endregion
}
