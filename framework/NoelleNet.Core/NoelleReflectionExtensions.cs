namespace System;

/// <summary>
/// 反射的扩展方法集
/// </summary>
public static class NoelleReflectionExtensions
{
    /// <summary>
    /// 获取当前泛型类型的类型名称
    /// </summary>
    /// <param name="source">源 <see cref="object"/> 实例</param>
    /// <returns></returns>
    public static string GetGenericTypeName(this Type source)
    {
        if (!source.IsGenericType)
            return source.Name;

        var genericTypes = source.GetGenericArguments().Select(s => s.Name);

        // MemberInfo.Name属性返回的值的结构为“MemberName`n”
        // MemberName为当前成员的名称，n为泛型参数的数量
        // 例如Dictionary<string,int>，返回的Name为Dictionary`2
        return $"{source.Name.Remove(source.Name.IndexOf('`'))}<{string.Join(",", genericTypes)}>";
    }

    /// <summary>
    /// 获取当前对象的泛型类型名称
    /// </summary>
    /// <param name="source">源 <see cref="object"/> 实例</param>
    /// <returns></returns>
    public static string GetGenericTypeName(this object source)
    {
        return GetGenericTypeName(source.GetType());
    }
}
