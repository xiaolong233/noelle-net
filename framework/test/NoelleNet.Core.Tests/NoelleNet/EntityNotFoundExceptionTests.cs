namespace NoelleNet;

/// <summary>
/// <see cref="EntityNotFoundException"/> 与 <see cref="EntityNotFoundException{TEntityType}"/> 的契约测试：
/// 异常处理器依赖 EntityType/Id 与消息格式生成 404 ProblemDetails。
/// </summary>
public class EntityNotFoundExceptionTests
{
    /// <summary>
    /// 无参构造时 EntityType 与 Id 应为 null
    /// </summary>
    [Fact]
    public void Constructor_Default_ShouldHaveNoTypeAndId()
    {
        var ex = new EntityNotFoundException();

        Assert.Null(ex.EntityType);
        Assert.Null(ex.Id);
    }

    /// <summary>
    /// 仅指定实体类型（无 Id）时，消息应说明"未指定实体的标识符"
    /// </summary>
    [Fact]
    public void Constructor_WithEntityTypeOnly_ShouldIndicateMissingId()
    {
        var ex = new EntityNotFoundException(typeof(string));

        Assert.Equal(typeof(string), ex.EntityType);
        Assert.Contains("未指定实体的标识符", ex.Message);
    }

    /// <summary>
    /// 指定实体类型与 Id 时，消息应包含标识符值
    /// </summary>
    [Fact]
    public void Constructor_WithEntityTypeAndId_ShouldIncludeIdInMessage()
    {
        var ex = new EntityNotFoundException(typeof(string), 123);

        Assert.Equal(typeof(string), ex.EntityType);
        Assert.Equal(123, ex.Id);
        Assert.Contains("标识符：123", ex.Message);
    }

    /// <summary>
    /// 内部异常应被传播
    /// </summary>
    [Fact]
    public void Constructor_WithInnerException_ShouldPropagate()
    {
        var inner = new InvalidOperationException("inner");

        var ex = new EntityNotFoundException(typeof(string), 123, inner);

        Assert.Same(inner, ex.InnerException);
    }

    /// <summary>
    /// 泛型版本：自动携带实体类型，与 Id 组合生成消息
    /// </summary>
    [Fact]
    public void Generic_WithId_ShouldSetTypeAndMessage()
    {
        var ex = new EntityNotFoundException<string>(42);

        Assert.Equal(typeof(string), ex.EntityType);
        Assert.Equal(42, ex.Id);
        Assert.Contains("标识符：42", ex.Message);
    }
}
