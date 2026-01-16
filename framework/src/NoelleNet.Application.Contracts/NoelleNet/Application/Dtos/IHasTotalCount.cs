namespace NoelleNet.Application.Dtos;

/// <summary>
/// 定义包含总记录数的接口。
/// </summary>
public interface IHasTotalCount
{
    /// <summary>
    /// 总记录数
    /// </summary>
    long TotalCount { get; set; }
}
