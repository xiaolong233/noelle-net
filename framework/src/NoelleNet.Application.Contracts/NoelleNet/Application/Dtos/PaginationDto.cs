namespace NoelleNet.Application.Dtos;

/// <summary>
/// 实现了 <see cref="IPagination"/> 的数据传输对象。
/// </summary>
[Obsolete("请使用 PagingDto")]
public class PaginationDto : LimitDto, IPagination
{
    /// <inheritdoc/>
    public int Offset { get; set; }
}
