namespace NoelleNet.Application.Dtos;

/// <summary>
/// 定义分页结果的接口。
/// </summary>
/// <typeparam name="T">列表中元素的类型</typeparam>
public interface IPagedResult<T> : IListResult<T>, IHasTotalCount
{
}
