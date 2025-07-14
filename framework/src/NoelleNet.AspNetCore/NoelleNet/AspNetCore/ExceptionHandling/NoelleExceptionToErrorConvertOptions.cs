namespace NoelleNet.AspNetCore.ExceptionHandling;

/// <summary>
/// 异常信息转换选项
/// </summary>
public class NoelleExceptionToErrorConvertOptions
{
    /// <summary>
    /// 跟踪标识提供者
    /// </summary>
    public Func<string>? TraceIdProvider { get; set; }

    /// <summary>
    /// 是否包含调试信息
    /// </summary>
    public bool IncludeDebugInfo { get; set; }
}
