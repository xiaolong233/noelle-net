using NoelleNet.Ddd.Domain.Entities;

namespace NoelleNet.Auditing;

/// <summary>
/// 实现了 <see cref="IAuditableEntity{TUser}"/> 的实体基类
/// </summary>
/// <typeparam name="TIdentifier">表示实体标识符的类型</typeparam>
/// <typeparam name="TUser">表示创建和修改实体的用户的标识符的类型</typeparam>
public class AuditableEntity<TIdentifier, TUser> : Entity<TIdentifier>, IAuditableEntity<TUser>
{
    public DateTime CreatedAt { get; private set; }

    public TUser? CreatedBy { get; private set; }

    public DateTime? LastModifiedAt { get; private set; }

    public TUser? LastModifiedBy { get; private set; }

    public void SetCreationAudit(DateTime createdAt, TUser? createdBy)
    {
        if (CreatedAt != default)
            throw new InvalidOperationException("无法更新创建审计信息");

        CreatedAt = createdAt;
        CreatedBy = createdBy;
    }

    public void UpdateModificationAudit(DateTime lastModifiedAt, TUser? lastModifiedBy)
    {
        LastModifiedAt = lastModifiedAt;
        LastModifiedBy = lastModifiedBy;
    }
}
