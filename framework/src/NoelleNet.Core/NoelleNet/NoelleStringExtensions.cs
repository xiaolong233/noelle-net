namespace NoelleNet;

/// <summary>
/// <see cref="string"/> 的扩展方法集
/// </summary>
public static class NoelleStringExtensions
{
    /// <summary>
    /// 指定的字符串是 null 还是空字符串 ("")。
    /// </summary>
    /// <param name="source">源 <see cref="string"/> 实例</param>
    /// <returns></returns>
    public static bool IsNullOrEmpty(this string? source)
    {
        return string.IsNullOrEmpty(source);
    }

    /// <summary>
    /// 指定的字符串是 null、空字符串 ("") 还是仅由空白字符组成 ("  ")。
    /// </summary>
    /// <param name="source">源 <see cref="string"/> 实例</param>
    /// <returns></returns>
    public static bool IsNullOrWhiteSpace(this string? source)
    {
        return string.IsNullOrWhiteSpace(source);
    }
}
