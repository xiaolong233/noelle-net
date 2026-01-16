namespace NoelleNet.Application.Dtos;

/// <summary>
/// 实现了 <see cref="IListResult{T}"/> 的数据传输对象。
/// </summary>
/// <typeparam name="T"></typeparam>
public class ListResultDto<T> : IListResult<T>
{
    /// <summary>
    /// 创建一个新的 <see cref="ListResultDto{T}"/> 实例
    /// </summary>
    public ListResultDto()
    {
    }

    /// <summary>
    /// 创建一个新的 <see cref="ListResultDto{T}"/> 实例
    /// </summary>
    /// <param name="items">列表项</param>
    public ListResultDto(IReadOnlyList<T> items)
    {
        Items = items;
    }

    /// <inheritdoc/>
    public IReadOnlyList<T> Items { get; set; } = [];
}
