using System.Linq.Expressions;
using System.Reflection;

namespace System;

/// <summary>
/// <see cref="object"/> 的帮助类
/// </summary>
public static class NoelleObjectHelper
{
    private static readonly Dictionary<string, PropertyInfo> _propertyInfoCache = [];

    /// <summary>
    /// 尝试设置指定属性的值
    /// </summary>
    /// <typeparam name="TObject">源对象的类型</typeparam>
    /// <typeparam name="TValue">值的类型</typeparam>
    /// <param name="source">源对象</param>
    /// <param name="propertySelector">属性选择器</param>
    /// <param name="valueFactory">值的提供者</param>
    /// <returns></returns>
    public static bool TrySetProperty<TObject, TValue>(TObject source, Expression<Func<TObject, TValue>> propertySelector, Func<TObject, TValue> valueFactory)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (propertySelector.Body.NodeType != ExpressionType.MemberAccess)
            return false;

        var type = source.GetType();
        string cacheKey = $"{type.FullName}:{propertySelector}";
        if (!_propertyInfoCache.TryGetValue(cacheKey, out PropertyInfo? propertyInfo))
        {
            MemberExpression memberExpression = (MemberExpression)propertySelector.Body;
            propertyInfo = type.GetProperties().FirstOrDefault(s => s.Name == memberExpression.Member.Name);
            if (propertyInfo == null)
                return false;
            _propertyInfoCache[cacheKey] = propertyInfo;
        }

        if (!propertyInfo.CanWrite)
            return false;

        propertyInfo.SetValue(source, valueFactory(source));
        return true;
    }
}
