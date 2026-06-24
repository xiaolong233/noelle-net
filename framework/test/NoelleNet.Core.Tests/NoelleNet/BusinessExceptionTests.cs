using Microsoft.Extensions.Logging;
using NoelleNet.ExceptionHandling;
using NoelleNet.Logging;

namespace NoelleNet;

public class BusinessExceptionTests
{
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

    [Fact]
    public void Constructor_WithDefaults_ShouldUseWarningLogLevel()
    {
        var ex = new BusinessException("ERR001", "错误");

        Assert.Equal(LogLevel.Warning, ex.LogLevel);
    }

    [Fact]
    public void ShouldImplementInterfaces()
    {
        var ex = new BusinessException();

        Assert.IsAssignableFrom<IBusinessException>(ex);
        Assert.IsAssignableFrom<IHasErrorCode>(ex);
        Assert.IsAssignableFrom<IHasErrorDetails>(ex);
        Assert.IsAssignableFrom<IHasLogLevel>(ex);
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        var ex = new BusinessException();

        ex.ErrorCode = "NEW_CODE";
        ex.Details = "新详情";
        ex.LogLevel = LogLevel.Critical;

        Assert.Equal("NEW_CODE", ex.ErrorCode);
        Assert.Equal("新详情", ex.Details);
        Assert.Equal(LogLevel.Critical, ex.LogLevel);
    }

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
