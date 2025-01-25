namespace NoelleNet.AspNetCore.Queries;

/// <summary>
/// 分页查询和排序参数的数据传输对象
/// </summary>
public class NoellePaginationAndSortDto : NoellePaginationDto
{
    /// <summary>
    /// 排序方式，格式：field1 asc|desc,field2 asc|desc
    /// </summary>
    public string? Sort { get; set; }
}