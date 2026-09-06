namespace System;

/// <summary>
/// <see cref="NoelleObjectExtensions"/> 的单元测试：覆盖普通类型转换与 Guid 特殊处理两个分支
/// </summary>
public class NoelleObjectExtensionsTests
{
    /// <summary>
    /// 基础类型转换（string ↔ int、string → double/decimal/bool）应成功
    /// </summary>
    [Fact]
    public void To_BasicTypes_ShouldConvert()
    {
        Assert.Equal(123, ((object)"123").To<int>());
        Assert.Equal("123", ((object)123).To<string>());
        Assert.Equal(3.14, ((object)"3.14").To<double>());
        Assert.Equal(99.99m, ((object)"99.99").To<decimal>());
        Assert.True(((object)"true").To<bool>());
    }

    /// <summary>
    /// 源为 null 时：目标为非可空类型抛 ArgumentNullException，可空类型返回 null
    /// </summary>
    [Fact]
    public void To_NullSource_ShouldThrowForNonNullableAndReturnNullForNullable()
    {
        object? source = null;

        Assert.Throws<ArgumentNullException>(() => source!.To<int>());
        Assert.Null(source!.To<int?>());
    }

    /// <summary>
    /// Guid 分支：有效字符串应转换，空/空白字符串对可空 Guid 返回 null
    /// </summary>
    [Fact]
    public void To_Guid_ShouldConvertOrReturnNull()
    {
        var guid = Guid.NewGuid();

        Assert.Equal(guid, ((object)guid.ToString()).To<Guid>());
        Assert.Equal(guid, ((object)guid.ToString()).To<Guid?>());
        Assert.Null(((object)"").To<Guid?>());
        Assert.Null(((object)"   ").To<Guid?>());
    }

    /// <summary>
    /// DateTime 转换应成功
    /// </summary>
    [Fact]
    public void To_DateTime_ShouldConvert()
    {
        Assert.Equal(new DateTime(2024, 1, 15), ((object)"2024-01-15").To<DateTime>());
    }
}
