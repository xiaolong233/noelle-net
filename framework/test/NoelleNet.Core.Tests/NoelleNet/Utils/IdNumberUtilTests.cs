namespace NoelleNet.Utils;

/// <summary>
/// <see cref="IdNumberUtil"/> 的单元测试：身份证校验位算法与信息提取
/// </summary>
public class IdNumberUtilTests
{
    #region Validate

    /// <summary>
    /// null、空串或空白应返回 false
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_NullOrEmpty_ShouldReturnFalse(string? input)
    {
        Assert.False(IdNumberUtil.Validate(input));
    }

    /// <summary>
    /// 校验位正确的 18 位号码应通过
    /// </summary>
    [Fact]
    public void Validate_Valid18Digit_ShouldReturnTrue()
    {
        Assert.True(IdNumberUtil.Validate("110101199003072893"));
    }

    /// <summary>
    /// 校验位错误或长度不合法应返回 false
    /// </summary>
    [Theory]
    [InlineData("440106198712050055")] // 校验位错误
    [InlineData("12345678901234567")]  // 17 位
    [InlineData("000000000000000000")] // 校验位错误
    public void Validate_Invalid_ShouldReturnFalse(string input)
    {
        Assert.False(IdNumberUtil.Validate(input));
    }

    #endregion

    #region ConvertTo18DigitIdCard

    /// <summary>
    /// 输入为 null、空或长度不为 15 位时应抛出 ArgumentException
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("12345678901234567")] // 17 位
    public void ConvertTo18DigitIdCard_InvalidInput_ShouldThrow(string? input)
    {
        Assert.Throws<ArgumentException>(() => IdNumberUtil.ConvertTo18DigitIdCard(input));
    }

    /// <summary>
    /// 合法的 15 位号码应转换为 18 位
    /// </summary>
    [Fact]
    public void ConvertTo18DigitIdCard_Valid15Digit_ShouldReturn18Digit()
    {
        var result = IdNumberUtil.ConvertTo18DigitIdCard("440106871205005");

        Assert.Equal(18, result.Length);
    }

    #endregion

    #region GetBirthday

    /// <summary>
    /// null 或空串应返回 null
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void GetBirthday_NullOrEmpty_ShouldReturnNull(string? input)
    {
        Assert.Null(IdNumberUtil.GetBirthday(input));
    }

    /// <summary>
    /// 合法号码应提取出生日期；非法号码返回 null
    /// </summary>
    [Fact]
    public void GetBirthday_ShouldExtractOrReturnNull()
    {
        Assert.Equal(new DateTime(1990, 3, 7), IdNumberUtil.GetBirthday("110101199003072893"));
        Assert.Null(IdNumberUtil.GetBirthday("123456789012345678"));
    }

    #endregion

    #region GetGender

    /// <summary>
    /// null 或空串应返回未知（0）
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void GetGender_NullOrEmpty_ShouldReturnUnknown(string? input)
    {
        Assert.Equal(0, IdNumberUtil.GetGender(input));
    }

    /// <summary>
    /// 第 17 位奇数为男（1）、偶数为女（2）；非法号码返回未知（0）
    /// </summary>
    [Fact]
    public void GetGender_ShouldReturnBySequenceCode()
    {
        Assert.Equal(1, IdNumberUtil.GetGender("110101199003072893"));   // 序列码 9，奇数
        Assert.Equal(0, IdNumberUtil.GetGender("123456789012345678"));  // 非法
    }

    #endregion

    #region GetRegionCode

    /// <summary>
    /// 合法号码返回前 6 位地区码；null 或非法号码返回 null
    /// </summary>
    [Fact]
    public void GetRegionCode_ShouldReturnFirst6DigitsOrNull()
    {
        Assert.Equal("110101", IdNumberUtil.GetRegionCode("110101199003072893"));
        Assert.Null(IdNumberUtil.GetRegionCode("123456789012345678"));
        Assert.Null(IdNumberUtil.GetRegionCode(null));
    }

    #endregion
}
