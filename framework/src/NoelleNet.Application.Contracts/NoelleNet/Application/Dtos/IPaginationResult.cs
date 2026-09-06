namespace NoelleNet.Application.Dtos;

/// <summary>
/// 定义分页结果的接口。
/// </summary>
/// <typeparam name="T"><inheritdoc/></typeparam>
[Obsolete("请使用 IPagedResult<T>")]
public interface IPaginationResult<T> : IListResult<T>, IHasTotalCount
{
}
