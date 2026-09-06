namespace System.Collections.Generic;

/// <summary>
/// <see cref="NoelleEnumerableExtensions"/> 的单元测试
/// </summary>
public class NoelleEnumerableExtensionsTests
{
    /// <summary>
    /// 条件为 true 时应用过滤
    /// </summary>
    [Fact]
    public void WhereIf_ConditionTrue_ShouldApplyFilter()
    {
        var source = new[] { 1, 2, 3, 4, 5 };

        var result = source.WhereIf(true, x => x > 3);

        Assert.Equal(new[] { 4, 5 }, result);
    }

    /// <summary>
    /// 条件为 false 时应原样返回源序列（惰性、不过滤）
    /// </summary>
    [Fact]
    public void WhereIf_ConditionFalse_ShouldReturnSource()
    {
        var source = new[] { 1, 2, 3, 4, 5 };

        var result = source.WhereIf(false, x => x > 3);

        Assert.Equal(source, result);
    }

    /// <summary>
    /// 空序列在条件为 true 时返回空结果
    /// </summary>
    [Fact]
    public void WhereIf_EmptySource_ShouldReturnEmpty()
    {
        var result = Enumerable.Empty<int>().WhereIf(true, x => x > 0);

        Assert.Empty(result);
    }

    /// <summary>
    /// 源序列为 null 时应抛出 ArgumentNullException
    /// </summary>
    [Fact]
    public void WhereIf_NullSource_ShouldThrow()
    {
        IEnumerable<int>? source = null;

        Assert.Throws<ArgumentNullException>(() => source!.WhereIf(true, x => x > 0));
    }
}
