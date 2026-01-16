using NoelleNet.Auditing;

namespace NoelleNet.Application.Dtos;

/// <summary>
/// 实现了 <see cref="ICreationAudited"/> 的数据传输对象。
/// </summary>
public class CreationAuditedEntityDto : EntityDto, ICreationAudited
{
    /// <inheritdoc/>
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc/>
    public string? CreatedBy { get; set; }
}

/// <summary>
/// 实现了 <see cref="ICreationAudited"/> 的数据传输对象。
/// </summary>
/// <typeparam name="TKey">实体标识符的类型</typeparam>
public class CreationAuditedEntityDto<TKey> : EntityDto<TKey>, ICreationAudited
{
    /// <inheritdoc/>
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc/>
    public string? CreatedBy { get; set; }
}