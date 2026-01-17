namespace NoelleNet.AspNetCore.ExceptionHandling;

/// <summary>
/// 异常信息转换选项
/// </summary>
public class NoelleExceptionHandlingOptions
{
    /// <summary>
    /// 跟踪标识提供者
    /// </summary>
    public Func<string>? TraceIdProvider { get; set; }

    /// <summary>
    /// 包含异常详情信息
    /// </summary>
    public bool IncludeExceptionDetails { get; set; }

    /// <summary>
    /// 包含堆栈跟踪信息
    /// </summary>
    public bool IncludeStackTrace { get; set; }

    /// <summary>
    /// 包含异常中的附加数据
    /// </summary>
    public bool IncludeExceptionData { get; set; }
}
