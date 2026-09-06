namespace NoelleNet.AspNetCore.Routing;

/// <summary>
/// <see cref="NoelleRouteKebabCaseTransformer"/> 的契约测试：kebab-case 路由转换的正则边界
/// </summary>
public class NoelleRouteKebabCaseTransformerTests
{
    private readonly NoelleRouteKebabCaseTransformer _transformer = new();

    /// <summary>
    /// null 与空串的边界行为
    /// </summary>
    [Fact]
    public void TransformOutbound_NullOrEmpty_ShouldReturnSame()
    {
        Assert.Null(_transformer.TransformOutbound(null));
        Assert.Equal("", _transformer.TransformOutbound(""));
    }

    /// <summary>
    /// 常规命名转换：PascalCase、camelCase、单词、全小写、多词、连续大写
    /// </summary>
    [Theory]
    [InlineData("TodoItems", "todo-items")]
    [InlineData("todoItems", "todo-items")]
    [InlineData("Items", "items")]
    [InlineData("items", "items")]
    [InlineData("TodoItemDetails", "todo-item-details")]
    [InlineData("HelloWorldTest", "hello-world-test")]
    [InlineData("ABC", "abc")]
    [InlineData("Todo2Item", "todo2-item")]
    public void TransformOutbound_RegularNames_ShouldConvert(string input, string expected)
    {
        Assert.Equal(expected, _transformer.TransformOutbound(input));
    }

    /// <summary>
    /// 缩写词边界：UserID → user-id、URLValue → url-value、UserIDCard → user-id-card
    /// </summary>
    [Theory]
    [InlineData("UserID", "user-id")]
    [InlineData("URLValue", "url-value")]
    [InlineData("UserIDCard", "user-id-card")]
    public void TransformOutbound_Acronyms_ShouldSplit(string input, string expected)
    {
        Assert.Equal(expected, _transformer.TransformOutbound(input));
    }

    /// <summary>
    /// 已含下划线等非字母边界的内容不做二次转换（正则仅匹配大小写边界）
    /// </summary>
    [Fact]
    public void TransformOutbound_NonCamelBoundaries_ShouldRemain()
    {
        Assert.Equal("todo_items", _transformer.TransformOutbound("todo_items"));
    }
}
