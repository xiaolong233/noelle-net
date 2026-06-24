namespace System;

public class NoelleStringExtensionsTests
{
    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("hello", false)]
    [InlineData(" ", false)]
    public void IsNullOrEmpty_ShouldReturnExpected(string? input, bool expected)
    {
        Assert.Equal(expected, input.IsNullOrEmpty());
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("   ", true)]
    [InlineData("hello", false)]
    [InlineData(" hello ", false)]
    public void IsNullOrWhiteSpace_ShouldReturnExpected(string? input, bool expected)
    {
        Assert.Equal(expected, input.IsNullOrWhiteSpace());
    }
}
