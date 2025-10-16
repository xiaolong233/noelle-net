using NoelleNet.Auditing;
using System.ComponentModel.DataAnnotations;

namespace NoelleNet.Ddd.Domain.Entities.Auditing;

/// <summary>
/// 包含创建和修改审计信息的聚合根基类
/// </summary>
public abstract class AuditedAggregateRoot : CreationAuditedAggregateRoot, IAudited
{
    /// <inheritdoc/>
    public DateTime? LastModifiedAt { get; protected set; }

    /// <inheritdoc/>
    [MaxLength(64)]
    public string? LastModifiedBy { get; protected set; }
}

/// <summary>
/// 包含创建和修改审计信息的聚合根基类
/// </summary>
/// <typeparam name="TIdentifier">实体标识符的数据类型</typeparam>
public abstract class AuditedAggregateRoot<TIdentifier> : CreationAuditedAggregateRoot<TIdentifier>, IAudited
{
    /// <inheritdoc/>
    public DateTime? LastModifiedAt { get; protected set; }

    /// <inheritdoc/>
    [MaxLength(64)]
    public string? LastModifiedBy { get; protected set; }
}