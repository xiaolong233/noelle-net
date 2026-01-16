using NoelleNet.Auditing;

namespace NoelleNet.Application.Dtos;

/// <summary>
/// 实现了 <see cref="IAudited"/> 的数据传输对象。
/// </summary>
public class AuditedEntityDto : CreationAuditedEntityDto, IAudited
{
    /// <inheritdoc/>
    public DateTime? LastModifiedAt { get; set; }

    /// <inheritdoc/>
    public string? LastModifiedBy { get; set; }
}

/// <summary>
/// 实现了 <see cref="IAudited"/> 的数据传输对象。
/// </summary>
/// <typeparam name="TKey">实体标识符的类型</typeparam>
public class AuditedEntityDto<TKey> : CreationAuditedEntityDto<TKey>, IAudited
{
    /// <inheritdoc/>
    public DateTime? LastModifiedAt { get; set; }

    /// <inheritdoc/>
    public string? LastModifiedBy { get; set; }
}
