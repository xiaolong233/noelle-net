using NoelleNet.Auditing;

namespace NoelleNet.Ddd.Domain.Entities.Auditing;

/// <summary>
/// 实现了 <see cref="ICreationAudited"/> 的实体基类
/// </summary>
/// <typeparam name="TIdentifier">实体标识符的数据类型</typeparam>
public class CreationAuditedEntity<TIdentifier> : Entity<TIdentifier>, ICreationAudited
{
    /// <summary>
    /// 创建时间
    /// </summary>
    public virtual DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// 创建人的标识符
    /// </summary>
    public virtual Guid? CreatedBy { get; protected set; }
}

/// <summary>
/// 实现了 <see cref="ICreationAudited{TUser}"/> 的实体基类
/// </summary>
/// <typeparam name="TIdentifier">实体标识符的数据类型</typeparam>
/// <typeparam name="TCreatedBy">创建人标识符的数据类型</typeparam>
public class CreationAuditedEntity<TIdentifier, TCreatedBy> : Entity<TIdentifier>, ICreationAudited<TCreatedBy>
{
    /// <summary>
    /// 创建时间
    /// </summary>
    public virtual DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// 创建人的标识符
    /// </summary>
    public virtual TCreatedBy? CreatedBy { get; protected set; }
}