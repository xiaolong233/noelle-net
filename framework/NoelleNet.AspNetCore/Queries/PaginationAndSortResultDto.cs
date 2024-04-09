namespace NoelleNet.AspNetCore.Queries;

/// <summary>
/// 分页查询和排序的结果
/// </summary>
/// <typeparam name="TData">查询的数据的类型</typeparam>
/// <param name="TotalCount">总记录数</param>
/// <param name="Data">查询的数据</param>
public record PaginationAndSortResultDto<TData>(int TotalCount, IEnumerable<TData> Data);