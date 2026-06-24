using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace System.Linq;

public class NoelleQueryableExtensionsTests
{
    private readonly IQueryable<int> _source = new[] { 1, 2, 3, 4, 5 }.AsQueryable();

    [Fact]
    public void WhereIf_Expression_ConditionTrue_ShouldFilter()
    {
        var result = _source.WhereIf(true, (Expression<Func<int, bool>>)(x => x > 3));
        Assert.Equal(new[] { 4, 5 }, result);
    }

    [Fact]
    public void WhereIf_Expression_ConditionFalse_ShouldReturnSource()
    {
        var result = _source.WhereIf(false, (Expression<Func<int, bool>>)(x => x > 3));
        Assert.Equal(_source, result);
    }

    [Fact]
    public void WhereIf_Expression_NullSource_ShouldThrow()
    {
        IQueryable<int>? source = null;
        Assert.Throws<ArgumentNullException>(() => source!.WhereIf(true, (Expression<Func<int, bool>>)(x => true)));
    }

    [Fact]
    public void WhereIf_StringExpression_ConditionTrue_ShouldFilter()
    {
        var result = _source.WhereIf(true, "it > 3");
        Assert.Equal(new[] { 4, 5 }, result);
    }

    [Fact]
    public void WhereIf_StringExpression_ConditionFalse_ShouldReturnSource()
    {
        var result = _source.WhereIf(false, "it > 3");
        Assert.Equal(_source, result);
    }

    [Fact]
    public void WhereIf_WithParsingConfig_ShouldWork()
    {
        var result = _source.WhereIf(true, ParsingConfig.Default, "it > 3");
        Assert.Equal(new[] { 4, 5 }, result);
    }

    [Fact]
    public void WhereIf_WithParsingConfig_ConditionFalse_ShouldReturnSource()
    {
        var result = _source.WhereIf(false, ParsingConfig.Default, "it > 3");
        Assert.Equal(_source, result);
    }

    [Fact]
    public void WhereIf_WithParsingConfig_NullSource_ShouldThrow()
    {
        IQueryable<int>? source = null;
        Assert.Throws<ArgumentNullException>(() => source!.WhereIf(true, ParsingConfig.Default, "it > 0"));
    }

    [Fact]
    public void OrderByIf_ConditionTrue_ShouldSort()
    {
        var source = new[] { 3, 1, 2 }.AsQueryable();
        var result = source.OrderByIf(true, "it");

        Assert.Equal(new[] { 1, 2, 3 }, result);
    }

    [Fact]
    public void OrderByIf_ConditionFalse_ShouldReturnSource()
    {
        var source = new[] { 3, 1, 2 }.AsQueryable();
        var result = source.OrderByIf(false, "it");

        Assert.Equal(source, result);
    }

    [Fact]
    public void OrderByIf_NullSortString_ShouldBehaveAsEmptyString()
    {
        var source = new[] { 3, 1, 2 }.AsQueryable();
        // Dynamic LINQ treats null/empty sort as no-op, but may throw on empty
        Assert.Throws<ArgumentException>(() => source.OrderByIf(true, (string?)null));
    }

    [Fact]
    public void OrderByIf_NullSource_ShouldThrow()
    {
        IQueryable<int>? source = null;
        Assert.Throws<ArgumentNullException>(() => source!.OrderByIf(true, "it"));
    }

    [Fact]
    public void OrderByIf_DescendingSort_ShouldSortDescending()
    {
        var source = new[] { 1, 3, 2 }.AsQueryable();
        var result = source.OrderByIf(true, "it desc");

        Assert.Equal(new[] { 3, 2, 1 }, result);
    }
}
