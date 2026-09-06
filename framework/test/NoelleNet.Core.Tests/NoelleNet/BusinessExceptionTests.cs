using Microsoft.Extensions.Logging;

namespace NoelleNet;

/// <summary>
/// <see cref="BusinessException"/> 的单元测试
/// </summary>
public class BusinessExceptionTests
{
    /// <summary>
    /// 全参数构造应初始化错误码、消息、详情、内部异常与日志级别
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        var ex = new BusinessException("ERR001", "测试错误", "详细信息", new InvalidOperationException("inner"), LogLevel.Error);

        Assert.Equal("ERR001", ex.ErrorCode);
        Assert.Equal("测试错误", ex.Message);
        Assert.Equal("详细信息", ex.Details);
        Assert.NotNull(ex.InnerException);
        Assert.Equal(LogLevel.Error, ex.LogLevel);
    }

    /// <summary>
    /// 默认日志级别应为 Warning（异常处理器据此决定日志级别）
    /// </summary>
    [Fact]
    public void Constructor_WithDefaults_ShouldUseWarningLogLevel()
    {
        var ex = new BusinessException("ERR001", "错误");

        Assert.Equal(LogLevel.Warning, ex.LogLevel);
    }

    /// <summary>
    /// 参数允许为 null（错误码/详情/内部异常均可省略）
    /// </summary>
    [Fact]
    public void Constructor_WithNullParameters_ShouldAllowNulls()
    {
        var ex = new BusinessException(null, null, null, null);

        Assert.Null(ex.ErrorCode);
        Assert.Null(ex.Details);
        Assert.Null(ex.InnerException);
        Assert.Equal(LogLevel.Warning, ex.LogLevel);
    }
}
