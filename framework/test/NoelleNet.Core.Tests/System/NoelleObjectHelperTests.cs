namespace System;

/// <summary>
/// <see cref="NoelleObjectHelper"/> 的单元测试
/// </summary>
public class NoelleObjectHelperTests
{
    /// <summary>
    /// 有效属性应被设置并返回 true，值工厂接收源对象
    /// </summary>
    [Fact]
    public void TrySetProperty_ValidProperty_ShouldSetAndReturnTrue()
    {
        var source = new TestClass { Name = "original" };

        var result = NoelleObjectHelper.TrySetProperty(source, x => x.Name, src => src.Name + "_suffix");

        Assert.True(result);
        Assert.Equal("original_suffix", source.Name);
    }

    /// <summary>
    /// 非成员表达式应返回 false 而不设置
    /// </summary>
    [Fact]
    public void TrySetProperty_NonMemberExpression_ShouldReturnFalse()
    {
        var source = new TestClass();

        Assert.False(NoelleObjectHelper.TrySetProperty<TestClass, int>(source, x => 42, _ => 10));
    }

    /// <summary>
    /// 只读属性应返回 false
    /// </summary>
    [Fact]
    public void TrySetProperty_ReadOnlyProperty_ShouldReturnFalse()
    {
        var source = new TestClass();

        Assert.False(NoelleObjectHelper.TrySetProperty(source, x => x.ReadOnly, _ => "new"));
    }

    /// <summary>
    /// 源对象为 null 时应抛出 ArgumentNullException
    /// </summary>
    [Fact]
    public void TrySetProperty_NullSource_ShouldThrow()
    {
        TestClass? source = null;

        Assert.Throws<ArgumentNullException>(() =>
            NoelleObjectHelper.TrySetProperty(source!, x => x.Name, _ => "value"));
    }

    public class TestClass
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string ReadOnly => "fixed";
    }
}
