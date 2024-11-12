using System.Linq.Expressions;
using System.Reflection;

namespace System;

/// <summary>
/// <see cref="object"/> 的帮助类
/// </summary>
public static class NoelleObjectHelper
{
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
        if (propertySelector.Body.NodeType != ExpressionType.MemberAccess)
            return false;

        MemberExpression memberExpression = (MemberExpression)propertySelector.Body;
        PropertyInfo? propertyInfo = source?.GetType().GetProperties().FirstOrDefault(s => s.Name == memberExpression.Member.Name);
        if (propertyInfo == null)
            return false;

        propertyInfo.SetValue(source, valueFactory(source));
        return true;
    }
}
