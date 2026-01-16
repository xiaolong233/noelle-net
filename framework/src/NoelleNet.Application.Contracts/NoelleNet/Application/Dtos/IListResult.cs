namespace NoelleNet.Application.Dtos;

/// <summary>
/// 定义列表结果的接口。
/// </summary>
/// <typeparam name="T">列表中元素的类型</typeparam>
public interface IListResult<T>
{
    /// <summary>
    /// 列表项
    /// </summary>
    IReadOnlyList<T> Items { get; set; }
}
