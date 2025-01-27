using System.ComponentModel;

namespace System;

/// <summary>
/// <see cref="object"/> 的扩展方法集
/// </summary>
public static class NoelleObjectExtensions
{
    /// <summary>
    /// 将源对象转换为指定类型 <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="source">需要转换的源对象</param>
    /// <returns></returns>
    public static T To<T>(this object source)
    {
        Type targetType = typeof(T);

        // 检查是否为可空值类型
        bool isNullable = Nullable.GetUnderlyingType(targetType) != null;

        // 获取基础类型（去掉 Nullable 包装）
        Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        // 处理 null 值
        if (source == null)
        {
            if (isNullable)
                return default!; // 可空值类型返回 null
            throw new ArgumentNullException(nameof(source));
        }

        // 特殊处理 Guid 和 Guid?
        if (underlyingType == typeof(Guid))
        {
            string sourceString = source.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(sourceString))
            {
                if (isNullable)
                    return default!; // Guid? 返回 null
            }

            return (T)TypeDescriptor.GetConverter(underlyingType).ConvertFromInvariantString(sourceString)!;
        }

        return (T)Convert.ChangeType(source, underlyingType);
    }
}
