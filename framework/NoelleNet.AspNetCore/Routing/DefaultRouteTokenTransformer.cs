using System.Text.RegularExpressions;

namespace NoelleNet.AspNetCore.Routing;

/// <summary>
/// 默认的路由转换器
/// </summary>
public class DefaultRouteTokenTransformer : IOutboundParameterTransformer
{
    /// <summary>
    /// 转换出站
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public string? TransformOutbound(object? value)
    {
        if (value == null)
            return null;
        return Regex.Replace(value.ToString()!, "([a-z])([A-Z])", "$1-$2", RegexOptions.CultureInvariant).ToLower();
    }
}
