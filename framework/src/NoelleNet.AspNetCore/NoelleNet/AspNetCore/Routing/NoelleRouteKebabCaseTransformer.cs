using System.Text.RegularExpressions;

namespace NoelleNet.AspNetCore.Routing;

/// <summary>
/// 把路由转换成 KebabCase 格式字符串的实现
/// </summary>
public sealed partial class NoelleRouteKebabCaseTransformer : IOutboundParameterTransformer
{
    /// <summary>
    /// 把路由字符串内容转换成小写字母+连字符的格式
    /// </summary>
    /// <param name="value">要转换的路由值</param>
    /// <returns></returns>
    public string? TransformOutbound(object? value)
    {
        if (value is null)
            return null;
        // 使用固定文化转换大小写，避免土耳其语等文化下 "I" 被转成无点 "ı"，导致 URL 在不同环境下不一致
        return GenerateKebabCaseRegex().Replace(value.ToString()!, "$1$3-$2$4").ToLowerInvariant();
    }

    // 同时匹配两种边界：小写/数字→大写（如 todoItems），以及大写→大写+小写（如 URLValue），
    // 保证缩写词也能正确切分：URLValue → url-value、UserIDCard → user-id-card
    [GeneratedRegex("([a-z0-9])([A-Z])|([A-Z])([A-Z][a-z])", RegexOptions.CultureInvariant)]
    private static partial Regex GenerateKebabCaseRegex();
}
