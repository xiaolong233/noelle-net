namespace NoelleNet.Http;

public class NoelleErrorDtoTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        var dto = new NoelleErrorDto("错误信息");

        Assert.Equal("错误信息", dto.Message);
        Assert.Null(dto.Code);
        Assert.Null(dto.Target);
        Assert.Null(dto.Details);
        Assert.Null(dto.InnerError);
    }

    [Fact]
    public void Constructor_WithNullMessage_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new NoelleErrorDto(null!));
    }

    [Fact]
    public void Constructor_WithCodeAndMessage_ShouldSetBoth()
    {
        var dto = new NoelleErrorDto("ERR001", "错误");

        Assert.Equal("ERR001", dto.Code);
        Assert.Equal("错误", dto.Message);
    }

    [Fact]
    public void Constructor_WithNullCode_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new NoelleErrorDto(null!, "msg"));
    }

    [Fact]
    public void Constructor_WithNullMessageInTwoParamCtor_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new NoelleErrorDto("code", null!));
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        var dto = new NoelleErrorDto("msg")
        {
            Code = "CODE",
            Target = "field",
            Details = new[] { new NoelleErrorDto("detail") },
            InnerError = "inner"
        };

        Assert.Equal("CODE", dto.Code);
        Assert.Equal("field", dto.Target);
        Assert.NotNull(dto.Details);
        Assert.Single(dto.Details);
        Assert.Equal("inner", dto.InnerError);
    }
}
