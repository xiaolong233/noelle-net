namespace System.Linq;

/// <summary>
/// <see cref="IEnumerable"/> 的扩展方法集
/// </summary>
public static class NoelleEnumerableExtensions
{
    /// <summary>
    /// 只有在 condition 为 true 时，才会根据 predicate 进行过滤
    /// </summary>
    /// <typeparam name="T">泛型类型</typeparam>
    /// <param name="source">源 <see cref="IEnumerable"/> 实例</param>
    /// <param name="condition">条件</param>
    /// <param name="predicate">筛选表达式</param>
    /// <returns></returns>
    public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> source, bool condition, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);

        return condition ? source.Where(predicate) : source;
    }
}
