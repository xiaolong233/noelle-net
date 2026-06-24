using NoelleNet.Auditing;
using NoelleNet.Ddd.Domain.Entities.Auditing;
using NoelleNet.Ddd.Domain.Events;

namespace NoelleNet.Ddd.Domain.Entities.Auditing;

/// <summary>
/// 用于测试的审计聚合根实现
/// </summary>
internal class TestAuditedAggregateRoot : AuditedAggregateRoot
{
    public int Id { get; set; }

    public override object?[] GetIdentifiers() => [Id];
}

internal class TestAuditedAggregateRootWithId : AuditedAggregateRoot<Guid>
{
    public TestAuditedAggregateRootWithId() { }
}

public class AuditedAggregateRootTests
{
    #region AuditedAggregateRoot (无类型参数)

    [Fact]
    public void ShouldImplementIAudited()
    {
        var aggregate = new TestAuditedAggregateRoot { Id = 1 };

        Assert.IsAssignableFrom<IAudited>(aggregate);
    }

    [Fact]
    public void ShouldImplementICreationAudited()
    {
        var aggregate = new TestAuditedAggregateRoot { Id = 1 };

        Assert.IsAssignableFrom<ICreationAudited>(aggregate);
    }

    [Fact]
    public void ShouldImplementIModificationAudited()
    {
        var aggregate = new TestAuditedAggregateRoot { Id = 1 };

        Assert.IsAssignableFrom<IModificationAudited>(aggregate);
    }

    [Fact]
    public void ShouldImplementIAggregateRoot()
    {
        var aggregate = new TestAuditedAggregateRoot { Id = 1 };

        Assert.IsAssignableFrom<IAggregateRoot>(aggregate);
    }

    [Fact]
    public void ShouldImplementIHasDomainEvents()
    {
        var aggregate = new TestAuditedAggregateRoot { Id = 1 };

        Assert.IsAssignableFrom<IHasDomainEvents>(aggregate);
    }

    [Fact]
    public void LastModifiedAt_DefaultValue_ShouldBeNull()
    {
        var aggregate = new TestAuditedAggregateRoot { Id = 1 };

        Assert.Null(aggregate.LastModifiedAt);
    }

    [Fact]
    public void LastModifiedBy_DefaultValue_ShouldBeNull()
    {
        var aggregate = new TestAuditedAggregateRoot { Id = 1 };

        Assert.Null(aggregate.LastModifiedBy);
    }

    [Fact]
    public void CreatedAt_DefaultValue_ShouldBeDefaultDateTime()
    {
        var aggregate = new TestAuditedAggregateRoot { Id = 1 };

        Assert.Equal(default, aggregate.CreatedAt);
    }

    [Fact]
    public void CreatedBy_DefaultValue_ShouldBeNull()
    {
        var aggregate = new TestAuditedAggregateRoot { Id = 1 };

        Assert.Null(aggregate.CreatedBy);
    }

    [Fact]
    public void DomainEvents_ShouldBeEmptyByDefault()
    {
        var aggregate = new TestAuditedAggregateRoot { Id = 1 };

        Assert.Empty(aggregate.DomainEvents);
    }

    #endregion

    #region AuditedAggregateRoot<TIdentifier>

    [Fact]
    public void GenericAuditedAggregateRoot_ShouldImplementIAudited()
    {
        var aggregate = new TestAuditedAggregateRootWithId();

        Assert.IsAssignableFrom<IAudited>(aggregate);
    }

    [Fact]
    public void GenericAuditedAggregateRoot_ShouldImplementIAggregateRoot()
    {
        var aggregate = new TestAuditedAggregateRootWithId();

        Assert.IsAssignableFrom<IAggregateRoot>(aggregate);
    }

    [Fact]
    public void GenericAuditedAggregateRoot_ShouldImplementIEntityOfT()
    {
        var aggregate = new TestAuditedAggregateRootWithId();

        Assert.IsAssignableFrom<IEntity<Guid>>(aggregate);
    }

    [Fact]
    public void GenericAuditedAggregateRoot_IdDefaultValue_ShouldBeDefault()
    {
        var aggregate = new TestAuditedAggregateRootWithId();

        Assert.Equal(default, aggregate.Id);
    }

    [Fact]
    public void GenericAuditedAggregateRoot_LastModifiedAt_DefaultValue_ShouldBeNull()
    {
        var aggregate = new TestAuditedAggregateRootWithId();

        Assert.Null(aggregate.LastModifiedAt);
    }

    [Fact]
    public void GenericAuditedAggregateRoot_LastModifiedBy_DefaultValue_ShouldBeNull()
    {
        var aggregate = new TestAuditedAggregateRootWithId();

        Assert.Null(aggregate.LastModifiedBy);
    }

    #endregion
}
