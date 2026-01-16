namespace NoelleNet.Application.Dtos;

/// <summary>
/// 定义包含分页信息的接口。
/// </summary>
public interface IPagination : IHasLimit
{
    /// <summary>
    /// 跳过的记录数
    /// </summary>
    int Offset { get; set; }
}
