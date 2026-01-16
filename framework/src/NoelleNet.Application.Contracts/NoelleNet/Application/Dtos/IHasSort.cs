namespace NoelleNet.Application.Dtos;

/// <summary>
/// 定义包含排序信息的接口。
/// </summary>
public interface IHasSort
{
    /// <summary>
    /// 排序信息
    /// </summary>
    /// <example>
    /// "DisplayOrder"
    /// "DisplayOrder ASC"
    /// "DisplayOrder ASC, Name DESC"
    /// </example>
    string? Sort { get; set; }
}
