namespace NoelleNet.Application.Dtos;

/// <summary>
/// <see cref="PagingDto"/> 的单元测试
/// </summary>
public class PagingDtoTests
{
    #region Offset 钳制

    /// <summary>
    /// 未赋值时，Offset 应为 0
    /// </summary>
    [Fact]
    public void Offset_NotAssigned_ShouldBeZero()
    {
        var dto = new PagingDto();

        Assert.Equal(0, dto.Offset);
    }

    /// <summary>
    /// 赋值为负数时，应钳制为 0（与 Limit 的宽容钳制哲学一致）
    /// </summary>
    [Fact]
    public void Offset_Negative_ShouldClampToZero()
    {
        var dto = new PagingDto { Offset = -1 };

        Assert.Equal(0, dto.Offset);
    }

    /// <summary>
    /// 赋值为正数时，应保持原值
    /// </summary>
    [Fact]
    public void Offset_Positive_ShouldKeepValue()
    {
        var dto = new PagingDto { Offset = 100 };

        Assert.Equal(100, dto.Offset);
    }

    #endregion

    #region 其他成员

    /// <summary>
    /// 未赋值时，Sort 应为 null（服务端使用默认排序）
    /// </summary>
    [Fact]
    public void Sort_NotAssigned_ShouldBeNull()
    {
        var dto = new PagingDto();

        Assert.Null(dto.Sort);
    }

    /// <summary>
    /// PagingDto 应继承 LimitDto 的钳制能力并实现 IPaging 接口
    /// </summary>
    [Fact]
    public void PagingDto_ShouldImplementIPagingAndInheritLimitClamp()
    {
        var dto = new PagingDto { Limit = int.MaxValue, Offset = -5, Sort = "CreatedAt desc" };

        Assert.IsAssignableFrom<IPaging>(dto);
        Assert.Equal(LimitDto.MaxLimit, dto.Limit);   // 继承自 LimitDto 的钳制
        Assert.Equal(0, dto.Offset);                  // 负值钳制
        Assert.Equal("CreatedAt desc", dto.Sort);
    }

    #endregion

    #region virtual 重写

    /// <summary>
    /// 派生类重写 Offset 后，应使用自定义规则（如端点级最大偏移限制）
    /// </summary>
    [Fact]
    public void Offset_OverriddenInDerivedClass_ShouldUseCustomRule()
    {
        var dto = new MaxOffsetPagingDto { Offset = 1000 };

        Assert.Equal(100, dto.Offset);
    }

    /// <summary>
    /// 自定义最大偏移为 100 的派生 DTO
    /// </summary>
    private class MaxOffsetPagingDto : PagingDto
    {
        private int _offset;

        public override int Offset
        {
            get => _offset;
            set => _offset = Math.Clamp(value, 0, 100);
        }
    }

    #endregion
}
