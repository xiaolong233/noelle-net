namespace NoelleNet.Application.Dtos;

/// <summary>
/// 实现了 <see cref="IPaginationResult{T}"/> 的数据传输对象。
/// </summary>
/// <typeparam name="T"></typeparam>
public class PaginationResultDto<T> : ListResultDto<T>, IPaginationResult<T>
{
    /// <summary>
    /// 创建一个新的 <see cref="PaginationResultDto{T}"/> 实例
    /// </summary>
    public PaginationResultDto()
    {
    }

    /// <summary>
    /// 创建一个新的 <see cref="PaginationResultDto{T}"/> 实例
    /// </summary>
    /// <param name="totalCount">总记录数</param>
    /// <param name="items">列表项</param>
    public PaginationResultDto(long totalCount, IReadOnlyList<T> items) : base(items)
    {
        TotalCount = totalCount;
    }

    /// <inheritdoc/>
    public long TotalCount { get; set; }
}
