namespace NoelleNet.Application.Dtos;

/// <summary>
/// 定义包含分页与排序信息的接口。
/// 分页模型为 offset-based（偏移量分页）：跳过 <see cref="Offset"/> 条记录后返回 <see cref="IHasLimit.Limit"/> 条记录，
/// 对应 SQL 的 LIMIT/OFFSET 与 OData 的 $skip/$top 语义。
/// </summary>
public interface IPaging : IHasLimit, IHasSort
{
    /// <summary>
    /// 跳过的记录数（offset-based 分页模型的偏移量），小于 0 时按 0 处理
    /// </summary>
    int Offset { get; set; }
}
