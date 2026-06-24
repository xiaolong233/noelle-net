namespace NoelleNet.Http;

public class RemoteCallValidationErrorInfoTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        var info = new RemoteCallValidationErrorInfo("字段不能为空");

        Assert.Equal("字段不能为空", info.Message);
        Assert.NotNull(info.Members);
        Assert.Empty(info.Members);
    }

    [Fact]
    public void Constructor_WithMessageAndMembers_ShouldSetBoth()
    {
        var info = new RemoteCallValidationErrorInfo("格式不正确", new[] { "Email", "Phone" });

        Assert.Equal("格式不正确", info.Message);
        Assert.Equal(new[] { "Email", "Phone" }, info.Members);
    }

    [Fact]
    public void Constructor_WithMessageAndEmptyMembers_ShouldSetEmptyMembers()
    {
        var info = new RemoteCallValidationErrorInfo("错误", Enumerable.Empty<string>());

        Assert.Equal("错误", info.Message);
        Assert.Empty(info.Members);
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        var info = new RemoteCallValidationErrorInfo("初始消息")
        {
            Message = "更新消息",
            Members = new[] { "Field1", "Field2" }
        };

        Assert.Equal("更新消息", info.Message);
        Assert.Equal(new[] { "Field1", "Field2" }, info.Members);
    }
}
