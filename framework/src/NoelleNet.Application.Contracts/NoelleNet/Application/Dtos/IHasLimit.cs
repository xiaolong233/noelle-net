namespace NoelleNet.Application.Dtos;

/// <summary>
/// 定义包含返回的记录数的接口。
/// </summary>
public interface IHasLimit
{
    /// <summary>
    /// 返回的记录数
    /// </summary>
    int Limit { get; set; }
}
