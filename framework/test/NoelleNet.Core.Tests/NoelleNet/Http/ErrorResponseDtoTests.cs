namespace NoelleNet.Http;

public class ErrorResponseDtoTests
{
    [Fact]
    public void Constructor_ShouldSetError()
    {
        var error = new RemoteCallErrorInfo("测试错误", "CODE01");
        var dto = new ErrorResponseDto(error);

        Assert.Same(error, dto.Error);
        Assert.Equal("测试错误", dto.Error.Message);
    }

    [Fact]
    public void WithNullError_ShouldAllowNull()
    {
        var dto = new ErrorResponseDto(null!);
        Assert.Null(dto.Error);
    }

    [Fact]
    public void Equality_ShouldWorkAsRecord()
    {
        var error1 = new RemoteCallErrorInfo("err");
        var error2 = new RemoteCallErrorInfo("err");

        var dto1 = new ErrorResponseDto(error1);
        var dto2 = new ErrorResponseDto(error1);

        Assert.Equal(dto1, dto2);
    }

    [Fact]
    public void ToString_ShouldBeRecordStyle()
    {
        var error = new RemoteCallErrorInfo("测试");
        var dto = new ErrorResponseDto(error);

        var str = dto.ToString();
        Assert.NotNull(str);
        Assert.Contains("ErrorResponseDto", str);
    }
}
