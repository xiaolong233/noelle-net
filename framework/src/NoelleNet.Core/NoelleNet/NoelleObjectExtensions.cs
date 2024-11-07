using System.ComponentModel;

namespace System;

/// <summary>
/// <see cref="object"/> 的扩展方法集
/// </summary>
public static class NoelleObjectExtensions
{
    /// <summary>
    /// 返回当前对象转换成新类型的实例
    /// </summary>
    /// <typeparam name="T">目标类型</typeparam>
    /// <param name="source">源 <see cref="object"/> 实例</param>
    /// <returns></returns>
    public static T To<T>(this object source)
    {
        if (typeof(T) == typeof(Guid))
            return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(source.ToString() ?? string.Empty)!;

        return (T)Convert.ChangeType(source, typeof(T));
    }
}
