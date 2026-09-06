using NoelleNet.Auditing;

namespace NoelleNet.Ddd.Domain.Entities.Auditing;

/// <summary>
/// 审计实体基类（CreationAuditedEntity / AuditedEntity）的契约测试：
/// 审计属性默认值、protected setter 可反射设置（NoelleAuditInterceptor 依赖此能力）
/// </summary>
public class AuditedEntityTests
{
    /// <summary>
    /// 创建审计字段默认值：CreatedAt 为 default(DateTime)、CreatedBy 为 null
    /// </summary>
    [Fact]
    public void CreationAuditedEntity_Defaults_ShouldBeEmpty()
    {
        var entity = new TestCreationAuditedEntity { Id = 1 };

        Assert.Equal(default, entity.CreatedAt);
        Assert.Null(entity.CreatedBy);
    }

    /// <summary>
    /// 修改审计字段默认值：LastModifiedAt/LastModifiedBy 为 null
    /// </summary>
    [Fact]
    public void AuditedEntity_Defaults_ShouldBeNull()
    {
        var entity = new TestAuditedEntity { Id = 1 };

        Assert.Null(entity.LastModifiedAt);
        Assert.Null(entity.LastModifiedBy);
    }

    /// <summary>
    /// protected setter 可通过反射设置（审计拦截器经 NoelleObjectHelper 写入审计值的依赖能力）
    /// </summary>
    [Fact]
    public void ProtectedSetters_ShouldBeWritableViaReflection()
    {
        var entity = new TestAuditedEntity { Id = 1 };

        typeof(AuditedEntity).GetProperty(nameof(AuditedEntity.LastModifiedAt))!
            .SetValue(entity, new DateTime(2024, 6, 15));
        typeof(AuditedEntity).GetProperty(nameof(AuditedEntity.LastModifiedBy))!
            .SetValue(entity, "modifier456");

        Assert.Equal(new DateTime(2024, 6, 15), entity.LastModifiedAt);
        Assert.Equal("modifier456", entity.LastModifiedBy);
    }

    /// <summary>
    /// 泛型审计实体：Id 默认值、审计能力继承
    /// </summary>
    [Fact]
    public void GenericAuditedEntity_ShouldInheritAuditCapabilities()
    {
        var entity = new TestAuditedEntityWithId();

        Assert.Equal(default, entity.Id);
        Assert.Null(entity.LastModifiedAt);
        Assert.Null(entity.CreatedBy);
    }

    /// <summary>
    /// 创建审计实体
    /// </summary>
    internal class TestCreationAuditedEntity : CreationAuditedEntity
    {
        public int Id { get; set; }

        public override object?[] GetIdentifiers() => [Id];
    }

    /// <summary>
    /// 审计实体
    /// </summary>
    internal class TestAuditedEntity : AuditedEntity
    {
        public int Id { get; set; }

        public override object?[] GetIdentifiers() => [Id];
    }

    internal class TestAuditedEntityWithId : AuditedEntity<Guid>
    {
    }
}
