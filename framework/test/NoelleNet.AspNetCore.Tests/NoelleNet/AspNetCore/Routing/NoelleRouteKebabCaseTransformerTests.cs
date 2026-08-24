namespace NoelleNet.AspNetCore.Routing;

public class NoelleRouteKebabCaseTransformerTests
{
    private readonly NoelleRouteKebabCaseTransformer _transformer = new();

    [Fact]
    public void TransformOutbound_NullValue_ShouldReturnNull()
    {
        var result = _transformer.TransformOutbound(null);

        Assert.Null(result);
    }

    [Fact]
    public void TransformOutbound_PascalCase_ShouldConvertToKebabCase()
    {
        var result = _transformer.TransformOutbound("TodoItems");

        Assert.Equal("todo-items", result);
    }

    [Fact]
    public void TransformOutbound_CamelCase_ShouldConvertToKebabCase()
    {
        var result = _transformer.TransformOutbound("todoItems");

        Assert.Equal("todo-items", result);
    }

    [Fact]
    public void TransformOutbound_SingleWord_ShouldReturnLowercase()
    {
        var result = _transformer.TransformOutbound("Items");

        Assert.Equal("items", result);
    }

    [Fact]
    public void TransformOutbound_AllLowercase_ShouldReturnSame()
    {
        var result = _transformer.TransformOutbound("items");

        Assert.Equal("items", result);
    }

    [Fact]
    public void TransformOutbound_MultipleWords_ShouldConvertAll()
    {
        var result = _transformer.TransformOutbound("TodoItemDetails");

        Assert.Equal("todo-item-details", result);
    }

    [Fact]
    public void TransformOutbound_AlreadyKebabCase_ShouldRemainLowercase()
    {
        var result = _transformer.TransformOutbound("todo_items");

        // Regex only matches lowercase followed by uppercase, so this won't change
        Assert.Equal("todo_items", result);
    }

    [Fact]
    public void TransformOutbound_Acronym_ShouldHandleCorrectly()
    {
        var result = _transformer.TransformOutbound("UserID");

        Assert.Equal("user-id", result);
    }

    [Fact]
    public void TransformOutbound_LeadingAcronym_ShouldHandleCorrectly()
    {
        var result = _transformer.TransformOutbound("URLValue");

        Assert.Equal("url-value", result);
    }

    [Fact]
    public void TransformOutbound_MiddleAcronym_ShouldHandleCorrectly()
    {
        var result = _transformer.TransformOutbound("UserIDCard");

        Assert.Equal("user-id-card", result);
    }

    [Fact]
    public void TransformOutbound_DigitWordBoundary_ShouldHandleCorrectly()
    {
        var result = _transformer.TransformOutbound("Todo2Item");

        Assert.Equal("todo2-item", result);
    }

    [Fact]
    public void TransformOutbound_AllCaps_ShouldConvertToLowercase()
    {
        var result = _transformer.TransformOutbound("ABC");

        Assert.Equal("abc", result);
    }

    [Fact]
    public void TransformOutbound_EmptyString_ShouldReturnEmpty()
    {
        var result = _transformer.TransformOutbound("");

        Assert.Equal("", result);
    }

    [Fact]
    public void TransformOutbound_ConsecutiveUppercase_ShouldHandle()
    {
        var result = _transformer.TransformOutbound("HelloWorldTest");

        Assert.Equal("hello-world-test", result);
    }
}
