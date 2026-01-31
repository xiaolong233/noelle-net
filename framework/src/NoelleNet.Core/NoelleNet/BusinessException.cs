using Microsoft.Extensions.Logging;
using NoelleNet.ExceptionHandling;
using NoelleNet.Logging;

namespace NoelleNet;

/// <summary>
/// 业务异常类
/// </summary>
public class BusinessException : Exception, IBusinessException, IHasErrorCode, IHasErrorDetails, IHasLogLevel
{
    /// <summary>
    /// 创建一个新的 <see cref="BusinessException"/> 实例
    /// </summary>
    /// <param name="errorCode">错误代码</param>
    /// <param name="message">错误信息</param>
    /// <param name="details">错误详情</param>
    /// <param name="innerException">内部异常</param>
    /// <param name="logLevel">日志等级</param>
    public BusinessException(
        string? errorCode = null,
        string? message = null,
        string? details = null,
        Exception? innerException = null,
        LogLevel logLevel = LogLevel.Warning)
        : base(message, innerException)
    {
        this.ErrorCode = errorCode;
        this.Details = details;
        this.LogLevel = logLevel;
    }

    /// <inheritdoc/>
    public string? ErrorCode { get; set; }

    /// <inheritdoc/>
    public string? Details { get; set; }

    /// <inheritdoc/>
    public LogLevel LogLevel { get; set; } = LogLevel.Warning;
}
