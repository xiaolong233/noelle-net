namespace NoelleNet.Application.Dtos;

/// <summary>
/// 定义分页结果的接口。
/// </summary>
/// <typeparam name="T"><inheritdoc/></typeparam>
public interface IPaginationResult<T> : IListResult<T>, IHasTotalCount
{
}
