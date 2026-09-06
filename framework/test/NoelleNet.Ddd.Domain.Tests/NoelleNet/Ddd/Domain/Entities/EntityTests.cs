namespace NoelleNet.Ddd.Domain.Entities;

#region 测试实体

/// <summary>
/// 单标识符测试实体
/// </summary>
internal class TestEntity : Entity
{
    public int Id { get; set; }

    public override object?[] GetIdentifiers() => [Id];
}

/// <summary>
/// 复合标识符测试实体
/// </summary>
internal class TestCompositeEntity : Entity
{
    public string FirstKey { get; set; } = "";
    public string SecondKey { get; set; } = "";

    public override object?[] GetIdentifiers() => [FirstKey, SecondKey];
}

/// <summary>
/// 无标识符测试实体
/// </summary>
internal class TestEmptyEntity : Entity
{
    public override object?[] GetIdentifiers() => [];
}

/// <summary>
/// null 标识符测试实体
/// </summary>
internal class TestNullIdentifierEntity : Entity
{
    public override object?[] GetIdentifiers() => null!;
}

internal class TestEntityWithGuid : Entity<Guid>
{
    public TestEntityWithGuid() { }

    public TestEntityWithGuid(Guid id) : base(id) { }
}

internal class TestEntityWithInt : Entity<int>
{
    public TestEntityWithInt() { }

    public TestEntityWithInt(int id) : base(id) { }
}

internal class TestEntityWithString : Entity<string>
{
    public TestEntityWithString() { }

    public TestEntityWithString(string id) : base(id) { }
}

internal class AnotherTestEntity : Entity
{
    public int Id { get; set; }

    public override object?[] GetIdentifiers() => [Id];
}

#endregion

/// <summary>
/// <see cref="Entity"/> 与 <see cref="Entity{TIdentifier}"/> 的契约测试：
/// 相等性语义（瞬态引用相等、非瞬态按标识符）、运算符、哈希与瞬态判定
/// </summary>
public class EntityTests
{
    #region Equals

    /// <summary>
    /// 同引用相等、与 null 不等
    /// </summary>
    [Fact]
    public void Equals_SameReferenceOrNull_ShouldFollowReferenceSemantics()
    {
        var entity = new TestEntity { Id = 1 };

        Assert.True(entity.Equals(entity));
        Assert.False(entity.Equals(null));
    }

    /// <summary>
    /// 同类型且标识符相同/不同
    /// </summary>
    [Fact]
    public void Equals_SameType_ShouldCompareByIdentifiers()
    {
        Assert.True(new TestEntity { Id = 1 }.Equals(new TestEntity { Id = 1 }));
        Assert.False(new TestEntity { Id = 1 }.Equals(new TestEntity { Id = 2 }));
    }

    /// <summary>
    /// 不同类型即使标识符相同也不相等
    /// </summary>
    [Fact]
    public void Equals_DifferentType_ShouldReturnFalse()
    {
        var entity1 = new TestEntity { Id = 1 };
        var entity2 = new AnotherTestEntity { Id = 1 };

        Assert.False(entity1.Equals(entity2));
    }

    /// <summary>
    /// 瞬态实体仅引用相等；瞬态与非瞬态不相等
    /// </summary>
    [Fact]
    public void Equals_TransientEntities_ShouldUseReferenceEquality()
    {
        var transient1 = new TestEntity { Id = 0 };
        var transient2 = new TestEntity { Id = 0 };
        var persistent = new TestEntity { Id = 1 };

        Assert.False(transient1.Equals(transient2));
        Assert.True(transient1.Equals(transient1));
        Assert.False(transient1.Equals(persistent));
        Assert.False(persistent.Equals(transient1));
    }

    /// <summary>
    /// 复合标识符按全部键比较
    /// </summary>
    [Fact]
    public void Equals_CompositeKey_ShouldCompareAllIdentifiers()
    {
        var entity1 = new TestCompositeEntity { FirstKey = "A", SecondKey = "B" };
        var entity2 = new TestCompositeEntity { FirstKey = "A", SecondKey = "B" };
        var entity3 = new TestCompositeEntity { FirstKey = "A", SecondKey = "C" };

        Assert.True(entity1.Equals(entity2));
        Assert.False(entity1.Equals(entity3));
    }

    #endregion

    #region Operators

    /// <summary>
    /// == 运算符：null 情况与标识符比较
    /// </summary>
    [Fact]
    public void EqualityOperator_ShouldHandleNullsAndIds()
    {
        TestEntity? left = null;
        TestEntity? right = null;
        Assert.True(left == right);

        var entity = new TestEntity { Id = 1 };
        Assert.False(left == entity);
        Assert.False(entity == null);

        Assert.True(entity == new TestEntity { Id = 1 });
        Assert.True(entity != new TestEntity { Id = 2 });
    }

    #endregion

    #region GetHashCode

    /// <summary>
    /// 相同标识符哈希相同、不同标识符哈希不同；空/null 标识符不抛异常
    /// </summary>
    [Fact]
    public void GetHashCode_ShouldFollowIdentifiers()
    {
        Assert.Equal(new TestEntity { Id = 1 }.GetHashCode(), new TestEntity { Id = 1 }.GetHashCode());
        Assert.NotEqual(new TestEntity { Id = 1 }.GetHashCode(), new TestEntity { Id = 2 }.GetHashCode());

        Assert.NotNull(new TestEmptyEntity().GetHashCode());
        Assert.NotNull(new TestNullIdentifierEntity().GetHashCode());
    }

    #endregion

    #region IsTransient

    /// <summary>
    /// 各类零值标识符（Guid.Empty/0/0L/null/空串）应判定为瞬态
    /// </summary>
    [Fact]
    public void IsTransient_EmptyIdentifiers_ShouldReturnTrue()
    {
        Assert.True(new TestEmptyEntity().IsTransient());
        Assert.True(new TestNullIdentifierEntity().IsTransient());
        Assert.True(new TestEntityWithGuid(Guid.Empty).IsTransient());
        Assert.True(new TestEntityWithInt(0).IsTransient());
        Assert.True(new TestEntityWithString(null!).IsTransient());
        Assert.True(new TestEntityWithString("").IsTransient());
        Assert.True(new TestEntityWithString("   ").IsTransient());
    }

    /// <summary>
    /// 非零标识符应判定为非瞬态
    /// </summary>
    [Fact]
    public void IsTransient_ValidIdentifiers_ShouldReturnFalse()
    {
        Assert.False(new TestEntityWithGuid(Guid.NewGuid()).IsTransient());
        Assert.False(new TestEntityWithInt(42).IsTransient());
        Assert.False(new TestEntityWithString("hello").IsTransient());
    }

    #endregion

    #region ToString

    /// <summary>
    /// ToString 应包含类型名与标识符（无/单/多标识符三种格式）
    /// </summary>
    [Fact]
    public void ToString_ShouldIncludeTypeAndIdentifiers()
    {
        Assert.Contains("[No Identifiers]", new TestEmptyEntity().ToString());

        var single = new TestEntity { Id = 42 }.ToString();
        Assert.StartsWith("TestEntity", single);
        Assert.Contains("[Id: 42]", single);

        var composite = new TestCompositeEntity { FirstKey = "A", SecondKey = "B" }.ToString();
        Assert.Contains("[Ids: A, B]", composite);

        Assert.Contains("[Id: null]", new TestEntityWithString(null!).ToString());
    }

    #endregion

    #region Entity<TIdentifier>

    /// <summary>
    /// 泛型实体：构造函数设置 Id、GetIdentifiers 返回 Id 数组、相等性按 Id
    /// </summary>
    [Fact]
    public void GenericEntity_ShouldSetIdAndCompareById()
    {
        var entity = new TestEntityWithInt(42);
        Assert.Equal(42, entity.Id);
        Assert.Equal([42], entity.GetIdentifiers());

        var guid = new Guid("11111111-1111-1111-1111-111111111111");
        Assert.True(new TestEntityWithGuid(guid).Equals(new TestEntityWithGuid(guid)));
        Assert.False(new TestEntityWithGuid(guid).Equals(new TestEntityWithGuid(Guid.NewGuid())));
    }

    #endregion
}
