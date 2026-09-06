namespace NoelleNet.Application.Dtos;

/// <summary>
/// <see cref="ListResultDto{T}"/> 的单元测试
/// </summary>
public class ListResultDtoTests
{
    /// <summary>
    /// 无参构造时，Items 应为空集合
    /// </summary>
    [Fact]
    public void ParameterlessConstructor_ShouldInitializeEmptyItems()
    {
        var dto = new ListResultDto<string>();

        Assert.NotNull(dto.Items);
        Assert.Empty(dto.Items);
    }

    /// <summary>
    /// 传入列表项时，应保留原集合
    /// </summary>
    [Fact]
    public void ConstructorWithItems_ShouldKeepItems()
    {
        var items = new List<string> { "a", "b" };

        var dto = new ListResultDto<string>(items);

        Assert.Same(items, dto.Items);
    }
}
