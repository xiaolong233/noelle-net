using Microsoft.Extensions.Logging;

namespace NoelleNet.Logging;

/// <summary>
/// 表示具有日志级别的接口
/// </summary>
public interface IHasLogLevel
{
    /// <summary>
    /// 日志等级
    /// </summary>
    LogLevel LogLevel { get; }
}
