namespace System;

public class NoelleExceptionExtensionsTests
{
    [Fact]
    public void WithData_ShouldAddDataAndReturnSameException()
    {
        var ex = new InvalidOperationException("test");

        var result = ex.WithData("key1", "value1");

        Assert.Same(ex, result);
        Assert.Equal("value1", ex.Data["key1"]);
    }

    [Fact]
    public void WithData_MultipleCalls_ShouldAccumulateData()
    {
        var ex = new InvalidOperationException("test");

        ex.WithData("key1", "value1")
          .WithData("key2", 42);

        Assert.Equal("value1", ex.Data["key1"]);
        Assert.Equal(42, ex.Data["key2"]);
    }

    [Fact]
    public void WithData_NullValue_ShouldStoreNull()
    {
        var ex = new InvalidOperationException("test");

        ex.WithData("key1", null);

        Assert.Null(ex.Data["key1"]);
    }

    [Fact]
    public void WithData_NullException_ShouldThrow()
    {
        InvalidOperationException? ex = null;
        Assert.Throws<ArgumentNullException>(() => ex!.WithData("key", "value"));
    }

    [Fact]
    public void WithData_NullName_ShouldThrow()
    {
        var ex = new InvalidOperationException("test");
        Assert.Throws<ArgumentNullException>(() => ex.WithData(null!, "value"));
    }

    [Fact]
    public void WithData_EmptyName_ShouldThrow()
    {
        var ex = new InvalidOperationException("test");
        Assert.Throws<ArgumentException>(() => ex.WithData("", "value"));
    }

    [Fact]
    public void WithData_WhitespaceName_ShouldThrow()
    {
        var ex = new InvalidOperationException("test");
        Assert.Throws<ArgumentException>(() => ex.WithData("   ", "value"));
    }

    [Fact]
    public void WithData_ShouldWorkWithDerivedExceptionTypes()
    {
        var ex = new ArgumentException("test");

        var result = ex.WithData("param", "testParam");

        Assert.Same(ex, result);
        Assert.Equal("testParam", ex.Data["param"]);
    }
}
