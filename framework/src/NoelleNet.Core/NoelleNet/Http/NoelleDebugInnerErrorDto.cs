namespace NoelleNet.Http;

/// <summary>
/// 包含调试信息的 <see cref="NoelleInnerErrorDto"/> 实现
/// </summary>
public class NoelleDebugInnerErrorDto : NoelleInnerErrorDto
{
    /// <summary>
    /// 创建一个新的 <see cref="NoelleDebugInnerErrorDto"/> 实例
    /// </summary>
    public NoelleDebugInnerErrorDto()
    {
    }

    /// <summary>
    /// 创建一个新的 <see cref="NoelleDebugInnerErrorDto"/> 实例
    /// </summary>
    /// <param name="code">错误代码</param>
    /// <param name="innerError">内部错误信息</param>
    public NoelleDebugInnerErrorDto(string? code, NoelleInnerErrorDto? innerError = null) : base(code, innerError)
    {
    }

    /// <summary>
    /// 创建一个新的 <see cref="NoelleDebugInnerErrorDto"/> 实例
    /// </summary>
    /// <param name="traceId">跟踪标识</param>
    /// <param name="exception">异常信息实例</param>
    public NoelleDebugInnerErrorDto(string? traceId, Exception? exception = null)
    {
        TraceId = traceId;
        Timestamp = DateTime.UtcNow;
        Message = exception?.Message;
        StackTrace = exception?.StackTrace;
    }

    /// <summary>
    /// 跟踪标识
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime? Timestamp { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 堆栈跟踪
    /// </summary>
    public string? StackTrace { get; set; }
}
