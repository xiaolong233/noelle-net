namespace NoelleNet.Application.Dtos;

/// <summary>
/// <see cref="PagedResultDto{T}"/> 的单元测试
/// </summary>
public class PagedResultDtoTests
{
    /// <summary>
    /// 无参构造时，Items 应为空集合、TotalCount 为 0
    /// </summary>
    [Fact]
    public void ParameterlessConstructor_ShouldInitializeEmptyItems()
    {
        var dto = new PagedResultDto<string>();

        Assert.NotNull(dto.Items);
        Assert.Empty(dto.Items);
        Assert.Equal(0, dto.TotalCount);
    }

    /// <summary>
    /// 传入总记录数与列表项时，应正确赋值
    /// </summary>
    [Fact]
    public void ConstructorWithValues_ShouldSetTotalCountAndItems()
    {
        var items = new List<string> { "a", "b" };

        var dto = new PagedResultDto<string>(2, items);

        Assert.Equal(2, dto.TotalCount);
        Assert.Same(items, dto.Items);
    }

    /// <summary>
    /// PagedResultDto 应实现 IPagedResult 接口
    /// </summary>
    [Fact]
    public void PagedResultDto_ShouldImplementIPagedResult()
    {
        var dto = new PagedResultDto<int>(1, [1]);

        Assert.IsAssignableFrom<IPagedResult<int>>(dto);
    }
}
