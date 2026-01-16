namespace NoelleNet.Application.Dtos;

/// <summary>
/// 实现了 <see cref="IEntityDto"/> 的数据传输对象。
/// </summary>
public class EntityDto : IEntityDto
{
    /// <inheritdoc/>
    public override string ToString()
    {
        return $"[DTO: {GetType().Name}]";
    }
}

/// <summary>
/// 实现了 <see cref="IEntityDto{TKey}"/> 的数据传输对象。
/// </summary>
/// <typeparam name="TKey">实体标识符的类型</typeparam>
public class EntityDto<TKey> : EntityDto, IEntityDto<TKey>
{
    /// <inheritdoc/>
    public TKey Id { get; set; } = default!;

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"[DTO: {GetType().Name}] Id = {Id}";
    }
}
