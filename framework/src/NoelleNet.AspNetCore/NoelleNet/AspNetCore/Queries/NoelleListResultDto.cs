namespace NoelleNet.AspNetCore.Queries;

/// <summary>
/// 列表结果的数据传输对象
/// </summary>
/// <typeparam name="T">列表数据项的数据类型</typeparam>
public class NoelleListResultDto<T>
{
    /// <summary>
    /// 创建一个新的 <see cref="NoelleListResultDto{T}"/> 实例
    /// </summary>
    /// <param name="items">列表数据项集合</param>
    /// <exception cref="ArgumentNullException"></exception>
    public NoelleListResultDto(IReadOnlyList<T> items)
    {
        Items = items ?? throw new ArgumentNullException(nameof(items));
    }

    /// <summary>
    /// 列表数据项集合
    /// </summary>
    public IReadOnlyList<T> Items { get; set; } = [];
}
