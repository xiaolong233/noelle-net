using System.Linq.Expressions;

namespace System.Linq;

/// <summary>
/// <see cref="IQueryable"/> 的扩展方法集
/// </summary>
public static class NoelleQueryableExtensions
{
    /// <summary>
    /// 只有在 condition 为 true 时，才会根据 predicate 进行过滤
    /// </summary>
    /// <typeparam name="T">泛型类型</typeparam>
    /// <param name="source"><see cref="IQueryable"/> 的实例</param>
    /// <param name="condition">条件</param>
    /// <param name="predicate">表达式</param>
    /// <returns></returns>
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, bool condition, Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);

        return condition ? source.Where(predicate) : source;
    }
}
