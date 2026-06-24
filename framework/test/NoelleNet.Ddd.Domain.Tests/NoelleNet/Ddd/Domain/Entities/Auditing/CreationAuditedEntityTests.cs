using NoelleNet.Auditing;
using NoelleNet.Ddd.Domain.Entities.Auditing;

namespace NoelleNet.Ddd.Domain.Entities.Auditing;

/// <summary>
/// 用于测试的创建审计实体实现
/// </summary>
internal class TestCreationAuditedEntity : CreationAuditedEntity
{
    public int Id { get; set; }

    public override object?[] GetIdentifiers() => [Id];
}

internal class TestCreationAuditedEntityWithId : CreationAuditedEntity<Guid>
{
    public TestCreationAuditedEntityWithId() { }
}

public class CreationAuditedEntityTests
{
    #region CreationAuditedEntity (无类型参数)

    [Fact]
    public void ShouldImplementICreationAudited()
    {
        var entity = new TestCreationAuditedEntity { Id = 1 };

        Assert.IsAssignableFrom<ICreationAudited>(entity);
    }

    [Fact]
    public void ShouldImplementIHasCreatedAt()
    {
        var entity = new TestCreationAuditedEntity { Id = 1 };

        Assert.IsAssignableFrom<IHasCreatedAt>(entity);
    }

    [Fact]
    public void ShouldImplementIMayHaveCreator()
    {
        var entity = new TestCreationAuditedEntity { Id = 1 };

        Assert.IsAssignableFrom<IMayHaveCreator>(entity);
    }

    [Fact]
    public void ShouldImplementIEntity()
    {
        var entity = new TestCreationAuditedEntity { Id = 1 };

        Assert.IsAssignableFrom<IEntity>(entity);
    }

    [Fact]
    public void CreatedAt_DefaultValue_ShouldBeDefaultDateTime()
    {
        var entity = new TestCreationAuditedEntity { Id = 1 };

        Assert.Equal(default, entity.CreatedAt);
    }

    [Fact]
    public void CreatedBy_DefaultValue_ShouldBeNull()
    {
        var entity = new TestCreationAuditedEntity { Id = 1 };

        Assert.Null(entity.CreatedBy);
    }

    [Fact]
    public void CreatedAt_ShouldBeSettableInDerivedClass()
    {
        var entity = new TestCreationAuditedEntity { Id = 1 };
        typeof(CreationAuditedEntity).GetProperty(nameof(CreationAuditedEntity.CreatedAt))!
            .SetValue(entity, new DateTime(2024, 1, 1));

        Assert.Equal(new DateTime(2024, 1, 1), entity.CreatedAt);
    }

    [Fact]
    public void CreatedBy_ShouldBeSettableInDerivedClass()
    {
        var entity = new TestCreationAuditedEntity { Id = 1 };
        typeof(CreationAuditedEntity).GetProperty(nameof(CreationAuditedEntity.CreatedBy))!
            .SetValue(entity, "user123");

        Assert.Equal("user123", entity.CreatedBy);
    }

    #endregion

    #region CreationAuditedEntity<TIdentifier>

    [Fact]
    public void GenericCreationAuditedEntity_ShouldImplementICreationAudited()
    {
        var entity = new TestCreationAuditedEntityWithId();

        Assert.IsAssignableFrom<ICreationAudited>(entity);
    }

    [Fact]
    public void GenericCreationAuditedEntity_ShouldImplementIEntityOfT()
    {
        var entity = new TestCreationAuditedEntityWithId();

        Assert.IsAssignableFrom<IEntity<Guid>>(entity);
    }

    [Fact]
    public void GenericCreationAuditedEntity_IdDefaultValue_ShouldBeDefault()
    {
        var entity = new TestCreationAuditedEntityWithId();

        Assert.Equal(default, entity.Id);
    }

    #endregion
}
