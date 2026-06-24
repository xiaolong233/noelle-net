using NoelleNet.Ddd.Domain.Entities;

namespace NoelleNet.Ddd.Domain.Entities;

/// <summary>
/// 用于测试的实体实现（无类型标识符）
/// </summary>
internal class TestEntity : Entity
{
    public int Id { get; set; }

    public override object?[] GetIdentifiers() => [Id];
}

/// <summary>
/// 用于测试的实体实现（多标识符）
/// </summary>
internal class TestCompositeEntity : Entity
{
    public string FirstKey { get; set; } = "";
    public string SecondKey { get; set; } = "";

    public override object?[] GetIdentifiers() => [FirstKey, SecondKey];
}

/// <summary>
/// 用于测试的实体实现（无标识符）
/// </summary>
internal class TestEmptyEntity : Entity
{
    public override object?[] GetIdentifiers() => [];
}

/// <summary>
/// 用于测试的实体实现（null 标识符）
/// </summary>
internal class TestNullIdentifierEntity : Entity
{
    public override object?[] GetIdentifiers() => null!;
}

/// <summary>
/// 用于测试的泛型实体实现
/// </summary>
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

internal class TestEntityWithLong : Entity<long>
{
    public TestEntityWithLong() { }

    public TestEntityWithLong(long id) : base(id) { }
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

public class EntityTests
{
    #region Equals

    [Fact]
    public void Equals_SameReference_ShouldReturnTrue()
    {
        var entity = new TestEntity { Id = 1 };

        Assert.True(entity.Equals(entity));
    }

    [Fact]
    public void Equals_NullOther_ShouldReturnFalse()
    {
        var entity = new TestEntity { Id = 1 };

        Assert.False(entity.Equals(null));
    }

    [Fact]
    public void Equals_SameTypeAndSameIds_ShouldReturnTrue()
    {
        var entity1 = new TestEntity { Id = 1 };
        var entity2 = new TestEntity { Id = 1 };

        Assert.True(entity1.Equals(entity2));
    }

    [Fact]
    public void Equals_SameTypeAndDifferentIds_ShouldReturnFalse()
    {
        var entity1 = new TestEntity { Id = 1 };
        var entity2 = new TestEntity { Id = 2 };

        Assert.False(entity1.Equals(entity2));
    }

    [Fact]
    public void Equals_DifferentTypeSameIds_ShouldReturnFalse()
    {
        var entity1 = new TestEntity { Id = 1 };
        var entity2 = new AnotherTestEntity { Id = 1 };

        Assert.False(entity1.Equals(entity2));
    }

    [Fact]
    public void Equals_BothTransientDifferentReferences_ShouldReturnFalse()
    {
        var entity1 = new TestEntity { Id = 0 };
        var entity2 = new TestEntity { Id = 0 };

        Assert.False(entity1.Equals(entity2));
    }

    [Fact]
    public void Equals_BothTransientSameReference_ShouldReturnTrue()
    {
        var entity = new TestEntity { Id = 0 };

        Assert.True(entity.Equals(entity));
    }

    [Fact]
    public void Equals_OneTransientOneNot_ShouldReturnFalse()
    {
        var transient = new TestEntity { Id = 0 };
        var persistent = new TestEntity { Id = 1 };

        Assert.False(transient.Equals(persistent));
        Assert.False(persistent.Equals(transient));
    }

    [Fact]
    public void Equals_CompositeKeySameValues_ShouldReturnTrue()
    {
        var entity1 = new TestCompositeEntity { FirstKey = "A", SecondKey = "B" };
        var entity2 = new TestCompositeEntity { FirstKey = "A", SecondKey = "B" };

        Assert.True(entity1.Equals(entity2));
    }

    [Fact]
    public void Equals_CompositeKeyDifferentValues_ShouldReturnFalse()
    {
        var entity1 = new TestCompositeEntity { FirstKey = "A", SecondKey = "B" };
        var entity2 = new TestCompositeEntity { FirstKey = "A", SecondKey = "C" };

        Assert.False(entity1.Equals(entity2));
    }

    [Fact]
    public void Equals_CompositeKeyDifferentLength_ShouldReturnFalse()
    {
        var entity = new TestCompositeEntity { FirstKey = "A", SecondKey = "B" };
        var singleKeyEntity = new TestEntity { Id = 1 };

        Assert.False(entity.Equals(singleKeyEntity));
    }

    #endregion

    #region Operators

    [Fact]
    public void EqualityOperator_BothNull_ShouldReturnTrue()
    {
        TestEntity? left = null;
        TestEntity? right = null;

        Assert.True(left == right);
    }

    [Fact]
    public void EqualityOperator_LeftNull_ShouldReturnFalse()
    {
        TestEntity? left = null;
        var right = new TestEntity { Id = 1 };

        Assert.False(left == right);
    }

    [Fact]
    public void EqualityOperator_RightNull_ShouldReturnFalse()
    {
        var left = new TestEntity { Id = 1 };
        TestEntity? right = null;

        Assert.False(left == right);
    }

    [Fact]
    public void EqualityOperator_SameIds_ShouldReturnTrue()
    {
        var left = new TestEntity { Id = 1 };
        var right = new TestEntity { Id = 1 };

        Assert.True(left == right);
    }

    [Fact]
    public void InequalityOperator_DifferentIds_ShouldReturnTrue()
    {
        var left = new TestEntity { Id = 1 };
        var right = new TestEntity { Id = 2 };

        Assert.True(left != right);
    }

    [Fact]
    public void InequalityOperator_SameIds_ShouldReturnFalse()
    {
        var left = new TestEntity { Id = 1 };
        var right = new TestEntity { Id = 1 };

        Assert.False(left != right);
    }

    #endregion

    #region GetHashCode

    [Fact]
    public void GetHashCode_SameIds_ShouldReturnSameHash()
    {
        var entity1 = new TestEntity { Id = 1 };
        var entity2 = new TestEntity { Id = 1 };

        Assert.Equal(entity1.GetHashCode(), entity2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentIds_ShouldReturnDifferentHash()
    {
        var entity1 = new TestEntity { Id = 1 };
        var entity2 = new TestEntity { Id = 2 };

        Assert.NotEqual(entity1.GetHashCode(), entity2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_EmptyIdentifiers_ShouldNotThrow()
    {
        var entity = new TestEmptyEntity();

        var hash = entity.GetHashCode();
    }

    [Fact]
    public void GetHashCode_NullIdentifiers_ShouldNotThrow()
    {
        var entity = new TestNullIdentifierEntity();

        var hash = entity.GetHashCode();
    }

    #endregion

    #region IsTransient

    [Fact]
    public void IsTransient_EmptyIdentifiers_ShouldReturnTrue()
    {
        var entity = new TestEmptyEntity();

        Assert.True(entity.IsTransient());
    }

    [Fact]
    public void IsTransient_NullIdentifiers_ShouldReturnTrue()
    {
        var entity = new TestNullIdentifierEntity();

        Assert.True(entity.IsTransient());
    }

    [Fact]
    public void IsTransient_GuidEmpty_ShouldReturnTrue()
    {
        var entity = new TestEntityWithGuid(Guid.Empty);

        Assert.True(entity.IsTransient());
    }

    [Fact]
    public void IsTransient_IntZero_ShouldReturnTrue()
    {
        var entity = new TestEntityWithInt(0);

        Assert.True(entity.IsTransient());
    }

    [Fact]
    public void IsTransient_LongZero_ShouldReturnTrue()
    {
        var entity = new TestEntityWithLong(0L);

        Assert.True(entity.IsTransient());
    }

    [Fact]
    public void IsTransient_NullString_ShouldReturnTrue()
    {
        var entity = new TestEntityWithString(null!);

        Assert.True(entity.IsTransient());
    }

    [Fact]
    public void IsTransient_EmptyString_ShouldReturnTrue()
    {
        var entity = new TestEntityWithString("");

        Assert.True(entity.IsTransient());
    }

    [Fact]
    public void IsTransient_WhitespaceString_ShouldReturnTrue()
    {
        var entity = new TestEntityWithString("   ");

        Assert.True(entity.IsTransient());
    }

    [Fact]
    public void IsTransient_ValidGuid_ShouldReturnFalse()
    {
        var entity = new TestEntityWithGuid(Guid.NewGuid());

        Assert.False(entity.IsTransient());
    }

    [Fact]
    public void IsTransient_NonZeroInt_ShouldReturnFalse()
    {
        var entity = new TestEntityWithInt(42);

        Assert.False(entity.IsTransient());
    }

    [Fact]
    public void IsTransient_NonZeroLong_ShouldReturnFalse()
    {
        var entity = new TestEntityWithLong(100L);

        Assert.False(entity.IsTransient());
    }

    [Fact]
    public void IsTransient_NonEmptyString_ShouldReturnFalse()
    {
        var entity = new TestEntityWithString("hello");

        Assert.False(entity.IsTransient());
    }

    #endregion

    #region ToString

    [Fact]
    public void ToString_NoIdentifiers_ShouldReturnNoIdentifiers()
    {
        var entity = new TestEmptyEntity();

        Assert.Contains("[No Identifiers]", entity.ToString());
    }

    [Fact]
    public void ToString_SingleIdentifier_ShouldReturnIdFormat()
    {
        var entity = new TestEntity { Id = 42 };

        var str = entity.ToString();
        Assert.Contains("[Id: 42]", str);
        Assert.StartsWith("TestEntity", str);
    }

    [Fact]
    public void ToString_MultipleIdentifiers_ShouldReturnIdsFormat()
    {
        var entity = new TestCompositeEntity { FirstKey = "A", SecondKey = "B" };

        var str = entity.ToString();
        Assert.Contains("[Ids: A, B]", str);
    }

    [Fact]
    public void ToString_NullIdentifier_ShouldDisplayNull()
    {
        var entity = new TestEntityWithString(null!);

        var str = entity.ToString();
        Assert.Contains("[Id: null]", str);
    }

    [Fact]
    public void ToString_GenericEntity_ShouldUseTypeName()
    {
        var entity = new TestEntityWithInt(1);

        var str = entity.ToString();
        Assert.StartsWith("TestEntityWithInt", str);
    }

    #endregion

    #region Entity<TIdentifier>

    [Fact]
    public void GenericEntity_DefaultConstructor_IdShouldBeDefault()
    {
        var entity = new TestEntityWithInt();

        Assert.Equal(default, entity.Id);
    }

    [Fact]
    public void GenericEntity_ConstructorWithId_ShouldSetId()
    {
        var entity = new TestEntityWithInt(42);

        Assert.Equal(42, entity.Id);
    }

    [Fact]
    public void GenericEntity_GuidConstructor_ShouldSetId()
    {
        var guid = Guid.NewGuid();
        var entity = new TestEntityWithGuid(guid);

        Assert.Equal(guid, entity.Id);
    }

    [Fact]
    public void GenericEntity_GetIdentifiers_ShouldReturnIdArray()
    {
        var entity = new TestEntityWithInt(42);

        var ids = entity.GetIdentifiers();

        Assert.Single(ids);
        Assert.Equal(42, ids[0]);
    }

    [Fact]
    public void GenericEntity_ImplementsIEntityOfT()
    {
        var entity = new TestEntityWithInt(1);

        Assert.IsAssignableFrom<IEntity<int>>(entity);
        Assert.IsAssignableFrom<IEntity>(entity);
    }

    #endregion

    #region Equality with Entity<TIdentifier>

    [Fact]
    public void GenericEntity_Equals_SameId_ShouldReturnTrue()
    {
        var entity1 = new TestEntityWithGuid(new Guid("11111111-1111-1111-1111-111111111111"));
        var entity2 = new TestEntityWithGuid(new Guid("11111111-1111-1111-1111-111111111111"));

        Assert.True(entity1.Equals(entity2));
    }

    [Fact]
    public void GenericEntity_Equals_DifferentId_ShouldReturnFalse()
    {
        var entity1 = new TestEntityWithGuid(new Guid("11111111-1111-1111-1111-111111111111"));
        var entity2 = new TestEntityWithGuid(new Guid("22222222-2222-2222-2222-222222222222"));

        Assert.False(entity1.Equals(entity2));
    }

    #endregion
}
