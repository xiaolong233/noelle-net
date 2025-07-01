using NoelleNet.Ddd.Domain.Entities;

namespace NoelleNet.Auditing;

/// <summary>
/// 实现了 <see cref="IAuditableEntity{TUser}"/> 的实体基类
/// </summary>
/// <typeparam name="TIdentifier">实体标识符的类型</typeparam>
/// <typeparam name="TUser">创建和修改实体的用户标识符的类型</typeparam>
public class AuditableEntityBase<TIdentifier, TUser> : Entity<TIdentifier>, IAuditableEntity<TUser>
{
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public TUser? CreatedBy { get; protected set; }

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime? LastModifiedAt { get; protected set; }

    /// <summary>
    /// 最后修改人
    /// </summary>
    public TUser? LastModifiedBy { get; protected set; }
}
