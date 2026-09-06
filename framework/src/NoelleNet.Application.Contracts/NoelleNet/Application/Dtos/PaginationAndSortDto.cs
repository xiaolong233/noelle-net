namespace NoelleNet.Application.Dtos;

/// <summary>
/// 实现了 <see cref="IPaginationAndSort"/> 的数据传输对象。
/// </summary>
[Obsolete("已并入 PagingDto")]
public class PaginationAndSortDto : PaginationDto, IPaginationAndSort
{
    /// <inheritdoc/>
    public string? Sort { get; set; }
}
