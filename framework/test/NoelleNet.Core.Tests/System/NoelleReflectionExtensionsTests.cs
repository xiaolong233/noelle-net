namespace System;

public class NoelleReflectionExtensionsTests
{
    [Fact]
    public void GetGenericTypeName_NonGenericType_ShouldReturnName()
    {
        var type = typeof(string);
        Assert.Equal("String", type.GetGenericTypeName());
    }

    [Fact]
    public void GetGenericTypeName_GenericType_ShouldReturnFormattedName()
    {
        var type = typeof(Dictionary<string, int>);
        Assert.Equal("Dictionary<String,Int32>", type.GetGenericTypeName());
    }

    [Fact]
    public void GetGenericTypeName_SingleGenericParam_ShouldReturnFormattedName()
    {
        var type = typeof(List<int>);
        Assert.Equal("List<Int32>", type.GetGenericTypeName());
    }

    [Fact]
    public void GetGenericTypeName_ObjectOverload_NonGenericType_ShouldReturnName()
    {
        Assert.Equal("String", "hello".GetGenericTypeName());
    }

    [Fact]
    public void GetGenericTypeName_ObjectOverload_GenericType_ShouldReturnFormattedName()
    {
        var list = new List<int>();
        Assert.Equal("List<Int32>", list.GetGenericTypeName());
    }
}
