using System.Collections;

namespace NoelleNet.Http;

public class RemoteCallErrorInfoTests
{
    [Fact]
    public void Constructor_Default_ShouldCreate()
    {
        var info = new RemoteCallErrorInfo();
        Assert.Null(info.Code);
        Assert.Null(info.Message);
        Assert.Null(info.Details);
        Assert.Null(info.Data);
        Assert.Null(info.ValidationErrors);
    }

    [Fact]
    public void Constructor_WithAllParameters_ShouldSetProperties()
    {
        var data = new Dictionary<string, object> { { "key", "val" } };
        var info = new RemoteCallErrorInfo("错误信息", "CODE01", "详情", data);

        Assert.Equal("错误信息", info.Message);
        Assert.Equal("CODE01", info.Code);
        Assert.Equal("详情", info.Details);
        Assert.Same(data, info.Data);
    }

    [Fact]
    public void Constructor_WithNullOptions_ShouldAllowNulls()
    {
        var info = new RemoteCallErrorInfo("msg", null, null, null);

        Assert.Equal("msg", info.Message);
        Assert.Null(info.Code);
        Assert.Null(info.Details);
        Assert.Null(info.Data);
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        var info = new RemoteCallErrorInfo
        {
            Code = "E002",
            Message = "test",
            Details = "detail",
            Data = new Dictionary<string, object>(),
            ValidationErrors = new[] { new RemoteCallValidationErrorInfo("字段必填") }
        };

        Assert.Equal("E002", info.Code);
        Assert.Equal("test", info.Message);
        Assert.Equal("detail", info.Details);
        Assert.NotNull(info.Data);
        Assert.NotNull(info.ValidationErrors);
        Assert.Single(info.ValidationErrors);
    }
}
