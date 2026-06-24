using System.Linq.Expressions;

namespace System;

public class NoelleObjectHelperTests
{
    [Fact]
    public void TrySetProperty_ValidProperty_ShouldSetAndReturnTrue()
    {
        var source = new TestClass { Name = "original" };

        var result = NoelleObjectHelper.TrySetProperty(source, x => x.Name, _ => "updated");

        Assert.True(result);
        Assert.Equal("updated", source.Name);
    }

    [Fact]
    public void TrySetProperty_NonMemberExpression_ShouldReturnFalse()
    {
        var source = new TestClass();

        var result = NoelleObjectHelper.TrySetProperty<TestClass, int>(source, x => 42, _ => 10);

        Assert.False(result);
    }

    [Fact]
    public void TrySetProperty_ReadOnlyProperty_ShouldReturnFalse()
    {
        var source = new TestClass();

        var result = NoelleObjectHelper.TrySetProperty(source, x => x.ReadOnly, _ => "new");

        Assert.False(result);
    }

    [Fact]
    public void TrySetProperty_NullSource_ShouldThrow()
    {
        TestClass? source = null;

        Assert.Throws<ArgumentNullException>(() =>
            NoelleObjectHelper.TrySetProperty(source!, x => x.Name, _ => "value"));
    }

    [Fact]
    public void TrySetProperty_NonExistentProperty_ShouldReturnFalse()
    {
        var source = new TestClass();
        // Using a valid member access that resolves to a name not in the type
        Expression<Func<TestClass, string>> expr = x => x.ToString()!;

        var result = NoelleObjectHelper.TrySetProperty(source, expr, _ => "value");
        Assert.False(result);
    }

    [Fact]
    public void TrySetProperty_ValueFactoryReceivesSource_ShouldWork()
    {
        var source = new TestClass { Name = "old" };

        NoelleObjectHelper.TrySetProperty(source, x => x.Name, src => src.Name + "_suffix");

        Assert.Equal("old_suffix", source.Name);
    }

    [Fact]
    public void TrySetProperty_IntProperty_ShouldWork()
    {
        var source = new TestClass { Age = 10 };

        NoelleObjectHelper.TrySetProperty(source, x => x.Age, _ => 20);

        Assert.Equal(20, source.Age);
    }

    public class TestClass
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string ReadOnly => "fixed";
    }
}
