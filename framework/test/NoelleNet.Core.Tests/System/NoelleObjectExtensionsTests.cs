namespace System;

public class NoelleObjectExtensionsTests
{
    [Fact]
    public void To_ConvertStringToInt_ShouldSucceed()
    {
        object source = "123";
        var result = source.To<int>();
        Assert.Equal(123, result);
    }

    [Fact]
    public void To_ConvertIntToString_ShouldSucceed()
    {
        object source = 123;
        var result = source.To<string>();
        Assert.Equal("123", result);
    }

    [Fact]
    public void To_ConvertStringToDouble_ShouldSucceed()
    {
        object source = "3.14";
        var result = source.To<double>();
        Assert.Equal(3.14, result);
    }

    [Fact]
    public void To_ConvertStringToDecimal_ShouldSucceed()
    {
        object source = "99.99";
        var result = source.To<decimal>();
        Assert.Equal(99.99m, result);
    }

    [Fact]
    public void To_ConvertStringToBool_ShouldSucceed()
    {
        object source = "true";
        var result = source.To<bool>();
        Assert.True(result);
    }

    [Fact]
    public void To_NullSource_NonNullableType_ShouldThrow()
    {
        object? source = null;
        Assert.Throws<ArgumentNullException>(() => source!.To<int>());
    }

    [Fact]
    public void To_NullSource_NullableType_ShouldReturnDefault()
    {
        object? source = null;
        var result = source.To<int?>();
        Assert.Null(result);
    }

    [Fact]
    public void To_ConvertStringToGuid_ShouldSucceed()
    {
        var guid = Guid.NewGuid();
        object source = guid.ToString();
        var result = source.To<Guid>();
        Assert.Equal(guid, result);
    }

    [Fact]
    public void To_ConvertEmptyStringToNullableGuid_ShouldReturnNull()
    {
        object source = "";
        var result = source.To<Guid?>();
        Assert.Null(result);
    }

    [Fact]
    public void To_ConvertWhitespaceStringToNullableGuid_ShouldReturnNull()
    {
        object source = "   ";
        var result = source.To<Guid?>();
        Assert.Null(result);
    }

    [Fact]
    public void To_ConvertValidStringToNullableGuid_ShouldSucceed()
    {
        var guid = Guid.NewGuid();
        object source = guid.ToString();
        var result = source.To<Guid?>();
        Assert.Equal(guid, result);
    }

    [Fact]
    public void To_ConvertStringToDateTime_ShouldSucceed()
    {
        object source = "2024-01-15";
        var result = source.To<DateTime>();
        Assert.Equal(new DateTime(2024, 1, 15), result);
    }
}
