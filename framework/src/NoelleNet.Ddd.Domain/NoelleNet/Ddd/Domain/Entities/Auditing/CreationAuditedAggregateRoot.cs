using NoelleNet.Auditing;
using System.ComponentModel.DataAnnotations;

namespace NoelleNet.Ddd.Domain.Entities.Auditing;

/// <summary>
/// 包含创建审计信息的聚合根基类
/// </summary>
public abstract class CreationAuditedAggregateRoot : AggregateRoot, ICreationAudited
{
    /// <inheritdoc/>
    public virtual DateTime CreatedAt { get; protected set; }

    /// <inheritdoc/>
    [MaxLength(64)]
    public virtual string? CreatedBy { get; protected set; }
}

/// <summary>
/// 包含创建审计信息的聚合根基类
/// </summary>
/// <typeparam name="TIdentifier">实体标识符的数据类型</typeparam>
public abstract class CreationAuditedAggregateRoot<TIdentifier> : AggregateRoot<TIdentifier>, ICreationAudited
{
    /// <inheritdoc/>
    public virtual DateTime CreatedAt { get; protected set; }

    /// <inheritdoc/>
    [MaxLength(64)]
    public virtual string? CreatedBy { get; protected set; }
}