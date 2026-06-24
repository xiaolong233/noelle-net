namespace NoelleNet;

public class EntityNotFoundExceptionTests
{
    [Fact]
    public void Constructor_Default_ShouldCreateInstance()
    {
        var ex = new EntityNotFoundException();

        Assert.NotNull(ex);
        Assert.Null(ex.EntityType);
        Assert.Null(ex.Id);
    }

    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        var ex = new EntityNotFoundException("实体未找到");

        Assert.Equal("实体未找到", ex.Message);
        Assert.Null(ex.EntityType);
        Assert.Null(ex.Id);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_ShouldSetBoth()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new EntityNotFoundException("实体未找到", inner);

        Assert.Equal("实体未找到", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void Constructor_WithEntityType_ShouldSetEntityType()
    {
        var ex = new EntityNotFoundException(typeof(string));

        Assert.Equal(typeof(string), ex.EntityType);
        Assert.Contains("未指定实体的标识符", ex.Message);
    }

    [Fact]
    public void Constructor_WithEntityTypeAndId_ShouldSetBoth()
    {
        var ex = new EntityNotFoundException(typeof(string), 123);

        Assert.Equal(typeof(string), ex.EntityType);
        Assert.Equal(123, ex.Id);
        Assert.Contains("标识符：123", ex.Message);
    }

    [Fact]
    public void Constructor_WithEntityTypeAndIdAndInnerException_ShouldSetAll()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new EntityNotFoundException(typeof(string), 123, inner);

        Assert.Equal(typeof(string), ex.EntityType);
        Assert.Equal(123, ex.Id);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void Constructor_WithNullId_ShouldGenerateAppropriateMessage()
    {
        var ex = new EntityNotFoundException(typeof(string), null);

        Assert.Contains("未指定实体的标识符", ex.Message);
    }

    [Fact]
    public void EntityTypeAndId_ShouldBeSettable()
    {
        var ex = new EntityNotFoundException();

        ex.EntityType = typeof(int);
        ex.Id = 456;

        Assert.Equal(typeof(int), ex.EntityType);
        Assert.Equal(456, ex.Id);
    }
}

public class EntityNotFoundExceptionGenericTests
{
    [Fact]
    public void Constructor_Default_ShouldSetEntityType()
    {
        var ex = new EntityNotFoundException<string>();

        Assert.Equal(typeof(string), ex.EntityType);
        Assert.Contains("未指定实体的标识符", ex.Message);
    }

    [Fact]
    public void Constructor_WithId_ShouldSetId()
    {
        var ex = new EntityNotFoundException<string>(42);

        Assert.Equal(typeof(string), ex.EntityType);
        Assert.Equal(42, ex.Id);
        Assert.Contains("标识符：42", ex.Message);
    }

    [Fact]
    public void Constructor_WithIdAndInnerException_ShouldSetAll()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new EntityNotFoundException<string>(42, inner);

        Assert.Equal(typeof(string), ex.EntityType);
        Assert.Equal(42, ex.Id);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void EntityNotFoundException_Generic_ShouldBeAssignableToBase()
    {
        var ex = new EntityNotFoundException<string>();

        Assert.IsAssignableFrom<EntityNotFoundException>(ex);
    }
}
