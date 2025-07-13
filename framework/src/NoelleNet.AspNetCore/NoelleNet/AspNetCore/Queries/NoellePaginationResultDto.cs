namespace NoelleNet.AspNetCore.Queries;

/// <summary>
/// 分页查询结果的数据传输对象
/// </summary>
/// <typeparam name="T">列表数据项的数据类型</typeparam>
public class NoellePaginationResultDto<T> : NoelleListResultDto<T>
{
    /// <summary>
    /// 创建一个新的 <see cref="NoellePaginationResultDto{T}"/> 实例
    /// </summary>
    /// <param name="totalCount">总记录数</param>
    /// <param name="items">查询到的列表项</param>
    public NoellePaginationResultDto(long totalCount, IReadOnlyList<T> items) : base(items)
    {
        TotalCount = totalCount;
    }

    /// <summary>
    /// 总记录数
    /// </summary>
    public long TotalCount { get; set; }
}