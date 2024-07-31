namespace NoelleNet.AspNetCore.Queries;

/// <summary>
/// 分页查询和排序的参数
/// </summary>
/// <param name="Filter"> 筛选内容 </param>
/// <param name="Offset"> 跳过多少条记录 </param>
/// <param name="Limit"> 返回多少条记录 </param>
/// <param name="Sort"> 排序方式，格式：field asc|desc </param>
public record NoellePaginationAndSortDto(string? Filter = default, int Offset = 0, int Limit = 10, string? Sort = default);
