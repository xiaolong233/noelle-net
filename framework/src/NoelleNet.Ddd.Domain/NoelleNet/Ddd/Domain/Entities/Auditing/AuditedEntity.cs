using NoelleNet.Auditing;
using System.ComponentModel.DataAnnotations;

namespace NoelleNet.Ddd.Domain.Entities.Auditing;

/// <summary>
/// 包含创建和修改审计信息的实体基类
/// </summary>
public abstract class AuditedEntity : CreationAuditedEntity, IAudited
{
    /// <inheritdoc/>
    public DateTime? LastModifiedAt { get; protected set; }

    /// <inheritdoc/>
    [MaxLength(64)]
    public string? LastModifiedBy { get; protected set; }
}

/// <summary>
/// 包含创建和修改审计信息的实体基类
/// </summary>
/// <typeparam name="TIdentifier">实体标识符的数据类型</typeparam>
public abstract class AuditedEntity<TIdentifier> : CreationAuditedEntity<TIdentifier>, IAudited
{
    /// <inheritdoc/>
    public virtual DateTime? LastModifiedAt { get; protected set; }

    /// <inheritdoc/>
    [MaxLength(64)]
    public virtual string? LastModifiedBy { get; protected set; }
}
