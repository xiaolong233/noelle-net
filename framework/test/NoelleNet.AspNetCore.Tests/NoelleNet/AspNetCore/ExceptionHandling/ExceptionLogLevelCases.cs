using Microsoft.Extensions.Logging;
using NoelleNet.Validation;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace NoelleNet.AspNetCore.ExceptionHandling;

/// <summary>
/// 日志级别决策表的共享用例。
/// <para>
/// Handler、Filter、Middleware 三处的日志级别决策实现是彼此独立的副本，对同一异常必须得出同一级别，
/// 因此用例表只在这里维护一份：任意一处实现发生漂移，三条测试都会失败并直接指出是哪一处。
/// </para>
/// </summary>
internal static class ExceptionLogLevelCases
{
    /// <summary>业务异常：由构造函数参数显式声明 Warning</summary>
    public const string Business = "business";

    /// <summary>验证异常：由 IHasLogLevel 显式声明 Information</summary>
    public const string Validation = "validation";

    /// <summary>实体未找到：由 IHasLogLevel 显式声明 Information</summary>
    public const string EntityNotFound = "entity-not-found";

    /// <summary>仅实现 IHasValidationResults、未声明级别的异常</summary>
    public const string ValidationResultsOnly = "validation-results-only";

    /// <summary>仅实现 IBusinessException、未声明级别的异常</summary>
    public const string BusinessMarkerOnly = "business-marker-only";

    /// <summary>数据库并发冲突：内置类型表映射为 Warning</summary>
    public const string DbConcurrency = "db-concurrency";

    /// <summary>依赖调用超时：内层 TimeoutException 使其区别于普通取消，映射为 Warning</summary>
    public const string Timeout = "timeout";

    /// <summary>普通取消（客户端断开、应用关闭、内部令牌）：映射为 Debug</summary>
    public const string Canceled = "canceled";

    /// <summary>未匹配任何规则的未知异常：兜底为 Error</summary>
    public const string Unknown = "unknown";

    /// <summary>
    /// 用例键与期望日志级别
    /// </summary>
    public static TheoryData<string, LogLevel> All => new()
    {
        { Business, LogLevel.Warning },
        { Validation, LogLevel.Information },
        { EntityNotFound, LogLevel.Information },
        { ValidationResultsOnly, LogLevel.Information },
        { BusinessMarkerOnly, LogLevel.Information },
        { DbConcurrency, LogLevel.Warning },
        { Timeout, LogLevel.Warning },
        { Canceled, LogLevel.Debug },
        { Unknown, LogLevel.Error }
    };

    /// <summary>
    /// 按用例键创建异常实例
    /// </summary>
    /// <param name="caseName">用例键</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static Exception Create(string caseName) => caseName switch
    {
        Business => new BusinessException("ERR001"),
        Validation => new NoelleValidationException([]),
        EntityNotFound => new EntityNotFoundException<object>(1),
        ValidationResultsOnly => new ValidationResultsOnlyException(),
        BusinessMarkerOnly => new BusinessMarkerOnlyException(),
        DbConcurrency => new DBConcurrencyException(),
        Timeout => new TaskCanceledException("timeout", new TimeoutException()),
        Canceled => new OperationCanceledException(),
        Unknown => new InvalidOperationException("boom"),
        _ => throw new ArgumentOutOfRangeException(nameof(caseName), caseName, "未知的日志级别用例")
    };

    /// <summary>
    /// 仅实现 IHasValidationResults、未实现 IHasLogLevel 的异常
    /// </summary>
    private sealed class ValidationResultsOnlyException : Exception, IHasValidationResults
    {
        public IEnumerable<ValidationResult> ValidationResults => [];
    }

    /// <summary>
    /// 仅实现 IBusinessException、未实现 IHasLogLevel 的异常
    /// </summary>
    private sealed class BusinessMarkerOnlyException : Exception, IBusinessException
    {
    }
}
