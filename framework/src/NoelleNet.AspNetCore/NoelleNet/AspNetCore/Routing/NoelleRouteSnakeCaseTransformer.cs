using System.Text.RegularExpressions;

namespace NoelleNet.AspNetCore.Routing;

/// <summary>
/// 把路由转换成 SnakeCase 格式字符串的实现
/// </summary>
public partial class NoelleRouteSnakeCaseTransformer : IOutboundParameterTransformer
{
    /// <summary>
    /// 把路由字符串内容转换成小写字母+下划线的格式
    /// </summary>
    /// <param name="value">要转换的路由值</param>
    /// <returns></returns>
    public string? TransformOutbound(object? value)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        return GenerateSnakeCaseRegex().Replace(value.ToString()!, "$1-$2").ToLower();
    }

    [GeneratedRegex("([a-z])([A-Z])", RegexOptions.CultureInvariant)]
    private static partial Regex GenerateSnakeCaseRegex();
}
