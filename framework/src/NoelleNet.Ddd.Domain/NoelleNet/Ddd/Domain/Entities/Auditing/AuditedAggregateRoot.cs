using NoelleNet.Auditing;

namespace NoelleNet.Ddd.Domain.Entities.Auditing;

/// <summary>
/// 实现了 <see cref="IAudited"/> 的聚合根基类
/// </summary>
/// <typeparam name="TIdentifier">实体标识符的数据类型</typeparam>
public class AuditedAggregateRoot<TIdentifier> : AggregateRoot<TIdentifier>, IAudited
{
    /// <summary>
    /// 创建时间
    /// </summary>
    public virtual DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// 创建人的标识符
    /// </summary>
    public virtual Guid? CreatedBy { get; protected set; }

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public virtual DateTime? LastModifiedAt { get; protected set; }

    /// <summary>
    /// 最后修改人的标识符
    /// </summary>
    public virtual Guid? LastModifiedBy { get; protected set; }
}

/// <summary>
/// 实现了 <see cref="IAudited{TUser}"/> 的聚合根基类
/// </summary>
/// <typeparam name="TIdentifier">实体标识符的数据类型</typeparam>
/// <typeparam name="TOperatorId">操作人标识符的数据类型</typeparam>
public class AuditedAggregateRoot<TIdentifier, TOperatorId> : AggregateRoot<TIdentifier>, IAudited<TOperatorId>
{
    /// <summary>
    /// 创建时间
    /// </summary>
    public virtual DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// 创建人的标识符
    /// </summary>
    public virtual TOperatorId? CreatedBy { get; protected set; }

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public virtual DateTime? LastModifiedAt { get; protected set; }

    /// <summary>
    /// 最后修改人的标识符
    /// </summary>
    public virtual TOperatorId? LastModifiedBy { get; protected set; }
}