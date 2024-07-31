namespace NoelleNet.AspNetCore.Queries;

/// <summary>
/// 分页查询和排序的参数
/// </summary>
public class PaginationAndSortDto
{
    /// <summary>
    /// 筛选内容
    /// </summary>
    public string? Filter { get; set; }

    /// <summary>
    /// 跳过多少条记录
    /// </summary>
    public int Offset { get; set; }

    /// <summary>
    /// 返回多少条记录
    /// </summary>
    public int Limit { get; set; } = 10;

    /// <summary>
    /// 排序方式，格式：field asc|desc
    /// </summary>
    public string? Sort { get; set; }
}
