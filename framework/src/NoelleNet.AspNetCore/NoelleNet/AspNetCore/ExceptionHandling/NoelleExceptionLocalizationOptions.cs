using Microsoft.Extensions.Localization;

namespace NoelleNet.AspNetCore.ExceptionHandling;

/// <summary>
/// 异常消息本地化选项的配置类
/// </summary>
public class NoelleExceptionLocalizationOptions
{
    /// <summary>
    /// 本地化资源来源
    /// </summary>
    [Obsolete("请使用LocalizerProvider")]
    public Dictionary<string, Type> ResourceSources { get; } = [];

    /// <summary>
    /// 创建 <see cref="IStringLocalizer"/> 的委托
    /// </summary>
    public Func<Exception, IStringLocalizerFactory, IStringLocalizer>? LocalizerProvider { get; set; }
}
