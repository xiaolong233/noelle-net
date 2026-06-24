using NoelleNet.ExceptionHandling;
using System.Collections;

namespace NoelleNet.Http;

public class NoelleRemoteCallExceptionTests
{
    [Fact]
    public void Constructor_Default_ShouldCreate()
    {
        var ex = new NoelleRemoteCallException();
        Assert.IsType<NoelleRemoteCallException>(ex);
    }

    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        var ex = new NoelleRemoteCallException("远程调用失败");
        Assert.Equal("远程调用失败", ex.Message);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_ShouldSetBoth()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new NoelleRemoteCallException("远程调用失败", inner);
        Assert.Equal("远程调用失败", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void Constructor_WithErrorInfo_ShouldSetErrorAndCopyData()
    {
        var data = new Dictionary<string, object> { { "custom", "value" } };
        var error = new RemoteCallErrorInfo("远程错误", "CODE001", "详情", data);
        var ex = new NoelleRemoteCallException(error);

        Assert.Same(error, ex.Error);
        Assert.Equal("远程错误", ex.Message);
        Assert.Equal("value", ex.Data["custom"]);
    }

    [Fact]
    public void Constructor_WithErrorInfoAndInnerException_ShouldSetAll()
    {
        var error = new RemoteCallErrorInfo("远程错误");
        var inner = new Exception("inner");
        var ex = new NoelleRemoteCallException(error, inner);

        Assert.Same(error, ex.Error);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void Constructor_WithNullError_ShouldThrowNullReferenceException()
    {
        // The constructor accesses error.Message without null check
        Assert.Throws<NullReferenceException>(() => new NoelleRemoteCallException((RemoteCallErrorInfo)null!));
    }

    [Fact]
    public void Constructor_WithErrorWithNullData_ShouldNotThrow()
    {
        var error = new RemoteCallErrorInfo("no data");
        var ex = new NoelleRemoteCallException(error);
        Assert.Null(error.Data);
    }

    [Fact]
    public void ShouldImplementInterfaces()
    {
        var ex = new NoelleRemoteCallException();
        Assert.IsAssignableFrom<IHasErrorCode>(ex);
        Assert.IsAssignableFrom<IHasHttpStatusCode>(ex);
        Assert.IsAssignableFrom<IHasErrorDetails>(ex);
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        var ex = new NoelleRemoteCallException
        {
            ErrorCode = "E001",
            StatusCode = 500,
            Details = "内部错误"
        };

        Assert.Equal("E001", ex.ErrorCode);
        Assert.Equal(500, ex.StatusCode);
        Assert.Equal("内部错误", ex.Details);
    }
}
