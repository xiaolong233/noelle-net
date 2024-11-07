using System.Linq.Dynamic.Core;
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
    /// <param name="source">源 <see cref="IQueryable"/> 实例</param>
    /// <param name="condition">条件</param>
    /// <param name="predicate">表达式</param>
    /// <returns></returns>
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, bool condition, Expression<Func<T, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);

        return condition ? source.Where(predicate) : source;
    }

    /// <summary>
    /// 只有在 condition 为 true 时，才会根据 predicate 进行过滤
    /// </summary>
    /// <typeparam name="T">泛型类型</typeparam>
    /// <param name="source">源 <see cref="IQueryable"/> 实例</param>
    /// <param name="condition">条件</param>
    /// <param name="predicate">表达式字符串</param>
    /// <param name="args">表达式的参数</param>
    /// <returns></returns>
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, bool condition, string predicate, params object?[] args)
    {
        return WhereIf(source, condition, ParsingConfig.Default, predicate, args);
    }

    /// <summary>
    /// 只有在 condition 为 true 时，才会根据 predicate 进行过滤
    /// </summary>
    /// <typeparam name="T">泛型类型</typeparam>
    /// <param name="source">源 <see cref="IQueryable"/> 实例</param>
    /// <param name="condition">条件</param>
    /// <param name="config"><see cref="Dynamic.Core.ParsingConfig"/> 的实例</param>
    /// <param name="predicate">表达式字符串</param>
    /// <param name="args">表达式的参数</param>
    /// <returns></returns>
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, bool condition, ParsingConfig config, string predicate, params object?[] args)
    {
        ArgumentNullException.ThrowIfNull(source);

        return condition ? DynamicQueryableExtensions.Where<T>(source, config, predicate, args) : source;
    }

    /// <summary>
    /// 只有在 condition 为 true 时，才会根据 sort 进行排序
    /// </summary>
    /// <typeparam name="T">泛型类型</typeparam>
    /// <param name="source">源 <see cref="IQueryable"/> 实例</param>
    /// <param name="condition">条件</param>
    /// <param name="sort">排序条件，支持"field1,field2..."、"field1,field2 desc..."、"field1 asc,field2 desc..."等</param>
    /// <returns></returns>
    public static IQueryable<T> OrderByIf<T>(this IQueryable<T> source, bool condition, string? sort)
    {
        ArgumentNullException.ThrowIfNull(source);

        return condition ? DynamicQueryableExtensions.OrderBy(source, sort ?? string.Empty) : source;
    }
}
