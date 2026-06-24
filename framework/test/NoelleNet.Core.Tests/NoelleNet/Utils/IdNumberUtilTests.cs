namespace NoelleNet.Utils;

public class IdNumberUtilTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_NullOrEmpty_ShouldReturnFalse(string? input)
    {
        Assert.False(IdNumberUtil.Validate(input));
    }

    [Fact]
    public void Validate_Valid18Digit_ShouldReturnTrue()
    {
        // 110101199003072893 is a valid 18-digit ID
        Assert.True(IdNumberUtil.Validate("110101199003072893"));
    }

    [Fact]
    public void Validate_InvalidChecksum_ShouldReturnFalse()
    {
        // 440106198712050055 has wrong checksum
        Assert.False(IdNumberUtil.Validate("440106198712050055"));
    }

    [Theory]
    [InlineData("123456789012345678")]
    [InlineData("000000000000000000")]
    public void Validate_Invalid18Digit_ShouldReturnFalse(string input)
    {
        Assert.False(IdNumberUtil.Validate(input));
    }

    [Fact]
    public void Validate_WrongLength_ShouldReturnFalse()
    {
        // 17 digits is neither 15 nor 18
        Assert.False(IdNumberUtil.Validate("12345678901234567"));
    }

    [Fact]
    public void ConvertTo18DigitIdCard_Null_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => IdNumberUtil.ConvertTo18DigitIdCard(null));
    }

    [Fact]
    public void ConvertTo18DigitIdCard_Empty_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => IdNumberUtil.ConvertTo18DigitIdCard(""));
    }

    [Theory]
    [InlineData("12345678901234567")] // 17位
    public void ConvertTo18DigitIdCard_WrongLength_ShouldThrow(string input)
    {
        Assert.Throws<ArgumentException>(() => IdNumberUtil.ConvertTo18DigitIdCard(input));
    }

    [Fact]
    public void ConvertTo18DigitIdCard_Valid15Digit_ShouldReturn18Digit()
    {
        // 使用一个已知的15位身份证号（示例），转换后应为18位
        var result = IdNumberUtil.ConvertTo18DigitIdCard("440106871205005");
        Assert.Equal(18, result.Length);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetBirthday_NullOrEmpty_ShouldReturnNull(string? input)
    {
        Assert.Null(IdNumberUtil.GetBirthday(input));
    }

    [Fact]
    public void GetBirthday_Valid18Digit_ShouldReturnBirthday()
    {
        var result = IdNumberUtil.GetBirthday("110101199003072893");
        Assert.Equal(new DateTime(1990, 3, 7), result);
    }

    [Fact]
    public void GetBirthday_InvalidIdCard_ShouldReturnNull()
    {
        Assert.Null(IdNumberUtil.GetBirthday("123456789012345678"));
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("   ", 0)]
    public void GetGender_NullOrEmpty_ShouldReturnUnknown(string? input, int expected)
    {
        Assert.Equal(expected, IdNumberUtil.GetGender(input));
    }

    [Fact]
    public void GetGender_Male_ShouldReturn1()
    {
        var result = IdNumberUtil.GetGender("110101199003072893");
        Assert.Equal(1, result);
    }

    [Fact]
    public void GetGender_InvalidIdCard_ShouldReturn0()
    {
        Assert.Equal(0, IdNumberUtil.GetGender("123456789012345678"));
    }

    [Fact]
    public void GetRegionCode_Valid_ShouldReturnFirst6Digits()
    {
        Assert.Equal("110101", IdNumberUtil.GetRegionCode("110101199003072893"));
    }

    [Fact]
    public void GetRegionCode_Invalid_ShouldReturnNull()
    {
        Assert.Null(IdNumberUtil.GetRegionCode("123456789012345678"));
    }

    [Fact]
    public void GetRegionCode_Null_ShouldReturnNull()
    {
        Assert.Null(IdNumberUtil.GetRegionCode(null));
    }
}
