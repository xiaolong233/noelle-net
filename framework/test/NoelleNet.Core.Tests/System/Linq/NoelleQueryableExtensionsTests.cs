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
}
