namespace NoelleNet.Application.Dtos;

/// <summary>
/// 实现了 <see cref="IPagedResult{T}"/> 的数据传输对象。
/// </summary>
/// <typeparam name="T">列表中元素的类型</typeparam>
public class PagedResultDto<T> : ListResultDto<T>, IPagedResult<T>
{
    /// <summary>
    /// 创建一个新的 <see cref="PagedResultDto{T}"/> 实例
    /// </summary>
    public PagedResultDto()
    {
    }

    /// <summary>
    /// 创建一个新的 <see cref="PagedResultDto{T}"/> 实例
    /// </summary>
    /// <param name="totalCount">总记录数</param>
    /// <param name="items">列表项</param>
    public PagedResultDto(long totalCount, IReadOnlyList<T> items) : base(items)
    {
        TotalCount = totalCount;
    }

    /// <inheritdoc/>
    public long TotalCount { get; set; }
}
