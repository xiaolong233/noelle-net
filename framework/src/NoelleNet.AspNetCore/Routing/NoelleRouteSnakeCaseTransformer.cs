using System.Text.RegularExpressions;

namespace NoelleNet.AspNetCore.Routing;

/// <summary>
/// 路由字符串转换成SnakeCase格式的实现
/// </summary>
public class NoelleRouteSnakeCaseTransformer : IOutboundParameterTransformer
{
    /// <summary>
    /// 把路由字符串内容转换成小写字母+下划线的格式
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public string? TransformOutbound(object? value)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        return Regex.Replace(value.ToString()!, "([a-z])([A-Z])", "$1-$2", RegexOptions.CultureInvariant).ToLower();
    }
}
