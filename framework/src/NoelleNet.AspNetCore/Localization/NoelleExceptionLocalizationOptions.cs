namespace NoelleNet.AspNetCore.Localization;

/// <summary>
/// 异常消息本地化选项的配置类
/// </summary>
public class NoelleExceptionLocalizationOptions
{
    /// <summary>
    /// 本地化资源来源
    /// </summary>
    public Dictionary<string, Type> ResourceSources { get; } = [];
}
