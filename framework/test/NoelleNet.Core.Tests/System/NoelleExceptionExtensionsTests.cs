namespace System;

/// <summary>
/// <see cref="NoelleExceptionExtensions"/> 的单元测试
/// </summary>
public class NoelleExceptionExtensionsTests
{
    /// <summary>
    /// 应写入 Data 并返回原异常（支持链式调用）
    /// </summary>
    [Fact]
    public void WithData_ShouldAddDataAndReturnSameException()
    {
        var ex = new InvalidOperationException("test");

        var result = ex.WithData("key1", "value1").WithData("key2", 42);

        Assert.Same(ex, result);
        Assert.Equal("value1", ex.Data["key1"]);
        Assert.Equal(42, ex.Data["key2"]);
    }

    /// <summary>
    /// 值可以为 null（用于异常处理本地化的占位参数）
    /// </summary>
    [Fact]
    public void WithData_NullValue_ShouldStoreNull()
    {
        var ex = new InvalidOperationException("test");

        ex.WithData("key1", null);

        Assert.Null(ex.Data["key1"]);
    }

    /// <summary>
    /// 异常为 null 时应抛出 ArgumentNullException
    /// </summary>
    [Fact]
    public void WithData_NullException_ShouldThrow()
    {
        InvalidOperationException? ex = null;

        Assert.Throws<ArgumentNullException>(() => ex!.WithData("key", "value"));
    }

    /// <summary>
    /// 键名为 null 或空白时应抛出异常
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void WithData_InvalidName_ShouldThrow(string? name)
    {
        var ex = new InvalidOperationException("test");

        Assert.ThrowsAny<ArgumentException>(() => ex.WithData(name!, "value"));
    }
}
