namespace System;

/// <summary>
/// <see cref="NoelleReflectionExtensions"/> 的单元测试
/// </summary>
public class NoelleReflectionExtensionsTests
{
    /// <summary>
    /// 非泛型类型应返回类型名
    /// </summary>
    [Fact]
    public void GetGenericTypeName_NonGenericType_ShouldReturnName()
    {
        Assert.Equal("String", typeof(string).GetGenericTypeName());
    }

    /// <summary>
    /// 泛型类型应返回带泛型参数的可读名称（含 object 重载）
    /// </summary>
    [Fact]
    public void GetGenericTypeName_GenericType_ShouldReturnFormattedName()
    {
        Assert.Equal("Dictionary<String,Int32>", typeof(Dictionary<string, int>).GetGenericTypeName());
        Assert.Equal("List<Int32>", typeof(List<int>).GetGenericTypeName());

        // object 重载
        Assert.Equal("List<Int32>", new List<int>().GetGenericTypeName());
    }
}
