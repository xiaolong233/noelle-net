namespace NoelleNet.Application.Dtos;

/// <summary>
/// 定义包含分页和排序信息的接口。
/// </summary>
[Obsolete("已并入 IPaging")]
public interface IPaginationAndSort : IPagination, IHasSort
{
}
