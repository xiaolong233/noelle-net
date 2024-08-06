using NoelleNet.Ddd.Domain.Entities;

namespace NoelleNet.Auditing;

/// <summary>
/// 实现了 <see cref="IAuditableEntity{TUser}"/> 的实体类
/// </summary>
/// <typeparam name="TIdentifier"></typeparam>
internal class AuditableEntity<TIdentifier> : Entity<TIdentifier>, IAuditableEntity<TIdentifier>
{
    private DateTime _createdAt;
    public DateTime CreatedAt => _createdAt;

    private TIdentifier _createdBy = default!;
    public TIdentifier CreatedBy => _createdBy;

    private DateTime? _lastModifiedAt;
    public DateTime? LastModifiedAt => _lastModifiedAt;

    private TIdentifier _lastModifiedBy = default!;
    public TIdentifier LastModifiedBy => _lastModifiedBy;

    public virtual void SetCreationAudit(DateTime createdAt, TIdentifier createdBy)
    {
        if (CreatedAt != default)
            throw new InvalidOperationException("无法更新创建审核");

        if (createdBy is object obj && obj == null)
            throw new ArgumentNullException(nameof(createdBy), "createdBy不能为null");
        if (createdBy is Guid guid && guid == Guid.Empty)
            throw new ArgumentException("createdBy不能为空", nameof(createdBy));

        _createdAt = createdAt;
        _createdBy = createdBy;
    }

    public virtual void UpdateModificationAudit(DateTime lastModifiedAt, TIdentifier lastModifiedBy)
    {
        _lastModifiedAt = lastModifiedAt;
        _lastModifiedBy = lastModifiedBy;
    }
}
