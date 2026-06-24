using NoelleNet.Auditing;
using NoelleNet.Ddd.Domain.Entities.Auditing;

namespace NoelleNet.Ddd.Domain.Entities.Auditing;

/// <summary>
/// 用于测试的审计实体实现
/// </summary>
internal class TestAuditedEntity : AuditedEntity
{
    public int Id { get; set; }

    public override object?[] GetIdentifiers() => [Id];
}

internal class TestAuditedEntityWithId : AuditedEntity<Guid>
{
    public TestAuditedEntityWithId() { }
}

public class AuditedEntityTests
{
    #region AuditedEntity (无类型参数)

    [Fact]
    public void ShouldImplementIAudited()
    {
        var entity = new TestAuditedEntity { Id = 1 };

        Assert.IsAssignableFrom<IAudited>(entity);
    }

    [Fact]
    public void ShouldImplementICreationAudited()
    {
        var entity = new TestAuditedEntity { Id = 1 };

        Assert.IsAssignableFrom<ICreationAudited>(entity);
    }

    [Fact]
    public void ShouldImplementIModificationAudited()
    {
        var entity = new TestAuditedEntity { Id = 1 };

        Assert.IsAssignableFrom<IModificationAudited>(entity);
    }

    [Fact]
    public void LastModifiedAt_DefaultValue_ShouldBeNull()
    {
        var entity = new TestAuditedEntity { Id = 1 };

        Assert.Null(entity.LastModifiedAt);
    }

    [Fact]
    public void LastModifiedBy_DefaultValue_ShouldBeNull()
    {
        var entity = new TestAuditedEntity { Id = 1 };

        Assert.Null(entity.LastModifiedBy);
    }

    [Fact]
    public void CreatedAt_DefaultValue_ShouldBeDefaultDateTime()
    {
        var entity = new TestAuditedEntity { Id = 1 };

        Assert.Equal(default, entity.CreatedAt);
    }

    [Fact]
    public void CreatedBy_DefaultValue_ShouldBeNull()
    {
        var entity = new TestAuditedEntity { Id = 1 };

        Assert.Null(entity.CreatedBy);
    }

    [Fact]
    public void LastModifiedAt_ShouldBeSettableInDerivedClass()
    {
        var entity = new TestAuditedEntity { Id = 1 };
        typeof(AuditedEntity).GetProperty(nameof(AuditedEntity.LastModifiedAt))!
            .SetValue(entity, new DateTime(2024, 6, 15));

        Assert.Equal(new DateTime(2024, 6, 15), entity.LastModifiedAt);
    }

    [Fact]
    public void LastModifiedBy_ShouldBeSettableInDerivedClass()
    {
        var entity = new TestAuditedEntity { Id = 1 };
        typeof(AuditedEntity).GetProperty(nameof(AuditedEntity.LastModifiedBy))!
            .SetValue(entity, "modifier456");

        Assert.Equal("modifier456", entity.LastModifiedBy);
    }

    #endregion

    #region AuditedEntity<TIdentifier>

    [Fact]
    public void GenericAuditedEntity_ShouldImplementIAudited()
    {
        var entity = new TestAuditedEntityWithId();

        Assert.IsAssignableFrom<IAudited>(entity);
    }

    [Fact]
    public void GenericAuditedEntity_ShouldImplementIEntityOfT()
    {
        var entity = new TestAuditedEntityWithId();

        Assert.IsAssignableFrom<IEntity<Guid>>(entity);
    }

    [Fact]
    public void GenericAuditedEntity_LastModifiedAt_DefaultValue_ShouldBeNull()
    {
        var entity = new TestAuditedEntityWithId();

        Assert.Null(entity.LastModifiedAt);
    }

    [Fact]
    public void GenericAuditedEntity_LastModifiedBy_DefaultValue_ShouldBeNull()
    {
        var entity = new TestAuditedEntityWithId();

        Assert.Null(entity.LastModifiedBy);
    }

    [Fact]
    public void GenericAuditedEntity_IdDefaultValue_ShouldBeDefault()
    {
        var entity = new TestAuditedEntityWithId();

        Assert.Equal(default, entity.Id);
    }

    #endregion
}
