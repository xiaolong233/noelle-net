namespace NoelleNet.Application.Dtos;

/// <summary>
/// <see cref="LimitDto"/> 的单元测试。
/// 测试会修改静态 <see cref="LimitDto.DefaultLimit"/>/<see cref="LimitDto.MaxLimit"/>，构造与释放时恢复默认值，避免污染其他用例。
/// </summary>
public class LimitDtoTests : IDisposable
{
    public LimitDtoTests()
    {
        LimitDto.DefaultLimit = 10;
        LimitDto.MaxLimit = 1000;
    }

    public void Dispose()
    {
        LimitDto.DefaultLimit = 10;
        LimitDto.MaxLimit = 1000;
    }

    #region Limit 钳制

    /// <summary>
    /// 未赋值时，Limit 应使用 DefaultLimit
    /// </summary>
    [Fact]
    public void Limit_NotAssigned_ShouldUseDefaultLimit()
    {
        var dto = new LimitDto();

        Assert.Equal(LimitDto.DefaultLimit, dto.Limit);
    }

    /// <summary>
    /// 赋值为 0 或负数时，应回退到 DefaultLimit（宽容钳制）
    /// </summary>
    [Fact]
    public void Limit_ZeroOrNegative_ShouldFallbackToDefault()
    {
        var dto = new LimitDto { Limit = 0 };
        Assert.Equal(LimitDto.DefaultLimit, dto.Limit);

        dto.Limit = -1;
        Assert.Equal(LimitDto.DefaultLimit, dto.Limit);
    }

    /// <summary>
    /// 赋值大于 MaxLimit 时，应截断到 MaxLimit
    /// </summary>
    [Fact]
    public void Limit_GreaterThanMax_ShouldClampToMax()
    {
        var dto = new LimitDto { Limit = int.MaxValue };

        Assert.Equal(LimitDto.MaxLimit, dto.Limit);
    }

    /// <summary>
    /// 赋值在 [DefaultLimit, MaxLimit] 区间内时，应保持原值
    /// </summary>
    [Fact]
    public void Limit_WithinRange_ShouldKeepValue()
    {
        var dto = new LimitDto { Limit = 25 };

        Assert.Equal(25, dto.Limit);
    }

    #endregion

    #region 静态边界

    /// <summary>
    /// DefaultLimit 设置为 0 或负数时应抛出 ArgumentOutOfRangeException
    /// </summary>
    [Fact]
    public void DefaultLimit_ZeroOrNegative_ShouldThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => LimitDto.DefaultLimit = 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => LimitDto.DefaultLimit = -1);
    }

    /// <summary>
    /// DefaultLimit 大于 MaxLimit 时应抛出 ArgumentOutOfRangeException
    /// </summary>
    [Fact]
    public void DefaultLimit_GreaterThanMax_ShouldThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => LimitDto.DefaultLimit = LimitDto.MaxLimit + 1);
    }

    /// <summary>
    /// MaxLimit 设置为 0 或负数时应抛出 ArgumentOutOfRangeException
    /// </summary>
    [Fact]
    public void MaxLimit_ZeroOrNegative_ShouldThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => LimitDto.MaxLimit = 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => LimitDto.MaxLimit = -1);
    }

    /// <summary>
    /// MaxLimit 小于 DefaultLimit 时应抛出 ArgumentOutOfRangeException
    /// </summary>
    [Fact]
    public void MaxLimit_LessThanDefault_ShouldThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => LimitDto.MaxLimit = LimitDto.DefaultLimit - 1);
    }

    /// <summary>
    /// 合法修改 DefaultLimit 后应影响新实例的默认值
    /// </summary>
    [Fact]
    public void DefaultLimit_ValidValue_ShouldTakeEffect()
    {
        LimitDto.DefaultLimit = 20;

        var dto = new LimitDto();

        Assert.Equal(20, dto.Limit);
    }

    #endregion

    #region virtual 重写

    /// <summary>
    /// 派生类重写 Limit 后，应使用自定义的钳制规则（按端点定制上限）
    /// </summary>
    [Fact]
    public void Limit_OverriddenInDerivedClass_ShouldUseCustomClamp()
    {
        var dto = new Max50LimitDto { Limit = 500 };

        Assert.Equal(50, dto.Limit);
    }

    /// <summary>
    /// 自定义上限为 50 的派生 DTO
    /// </summary>
    private class Max50LimitDto : LimitDto
    {
        private int _limit = DefaultLimit;

        public override int Limit
        {
            get => _limit;
            set => _limit = Math.Clamp(value, DefaultLimit, 50);
        }
    }

    #endregion
}
