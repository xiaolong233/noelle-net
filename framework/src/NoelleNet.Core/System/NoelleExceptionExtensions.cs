namespace System;

/// <summary>
/// <see cref="Exception"/> 扩展方法集
/// </summary>
public static class NoelleExceptionExtensions
{
    /// <summary>
    /// 为异常添加自定义数据并返回异常对象
    /// </summary>
    /// <typeparam name="T">异常类型</typeparam>
    /// <param name="e">要添加数据的异常对象</param>
    /// <param name="name">数据的键名称</param>
    /// <param name="value">数据的值，可以为空</param>
    /// <returns></returns>
    public static T WithData<T>(this T e, string name, object? value) where T : Exception
    {
        ArgumentNullException.ThrowIfNull(e, nameof(e));
        e.Data[name] = value;
        return e;
    }
}
