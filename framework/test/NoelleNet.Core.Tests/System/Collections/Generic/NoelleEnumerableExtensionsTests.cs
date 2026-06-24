namespace System.Collections.Generic;

public class NoelleEnumerableExtensionsTests
{
    [Fact]
    public void WhereIf_ConditionTrue_ShouldApplyFilter()
    {
        var source = new[] { 1, 2, 3, 4, 5 };

        var result = source.WhereIf(true, x => x > 3);

        Assert.Equal(new[] { 4, 5 }, result);
    }

    [Fact]
    public void WhereIf_ConditionFalse_ShouldReturnSource()
    {
        var source = new[] { 1, 2, 3, 4, 5 };

        var result = source.WhereIf(false, x => x > 3);

        Assert.Equal(source, result);
    }

    [Fact]
    public void WhereIf_NullSource_ShouldThrow()
    {
        IEnumerable<int>? source = null;
        Assert.Throws<ArgumentNullException>(() => source!.WhereIf(true, x => x > 0));
    }

    [Fact]
    public void WhereIf_EmptySource_ShouldReturnEmpty()
    {
        var source = Enumerable.Empty<int>();

        var result = source.WhereIf(true, x => x > 0);

        Assert.Empty(result);
    }

    [Fact]
    public void WhereIf_WithStrings_ShouldWork()
    {
        var source = new[] { "a", "bb", "ccc", "dddd" };

        var result = source.WhereIf(true, s => s.Length > 2);

        Assert.Equal(new[] { "ccc", "dddd" }, result);
    }
}
