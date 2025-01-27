namespace NoelleNet.AspNetCore.Queries;

/// <summary>
/// 分页查询结果的数据传输对象
/// </summary>
/// <typeparam name="T">查询到的列表项的数据类型</typeparam>
/// <param name="TotalCount">总记录数</param>
/// <param name="Items">查询到的列表项</param>
public record NoellePaginationResultDto<T>(long TotalCount, IReadOnlyList<T>? Items);