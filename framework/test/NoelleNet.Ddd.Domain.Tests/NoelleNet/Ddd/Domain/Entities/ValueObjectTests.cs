using NoelleNet.Ddd.Domain.Entities;

namespace NoelleNet.Ddd.Domain.Entities;

/// <summary>
/// 用于测试的值对象实现——地址
/// </summary>
internal class Address : ValueObject
{
    public string Street { get; set; } = "";
    public string City { get; set; } = "";
    public string ZipCode { get; set; } = "";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return ZipCode;
    }
}

/// <summary>
/// 用于测试的值对象实现——金额
/// </summary>
internal class Money : ValueObject
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}

/// <summary>
/// 用于测试的值对象实现——无组件
/// </summary>
internal class EmptyValueObject : ValueObject
{
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield break;
    }
}

/// <summary>
/// 与 Address 类型不同的值对象（用于类型区分测试）
/// </summary>
internal class AddressLike : ValueObject
{
    public string Street { get; set; } = "";
    public string City { get; set; } = "";
    public string ZipCode { get; set; } = "";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return ZipCode;
    }
}

public class ValueObjectTests
{
    #region Equals

    [Fact]
    public void Equals_SameReference_ShouldReturnTrue()
    {
        var address = new Address { Street = "Main St", City = "NYC", ZipCode = "10001" };

        Assert.True(address.Equals(address));
    }

    [Fact]
    public void Equals_Null_ShouldReturnFalse()
    {
        var address = new Address { Street = "Main St", City = "NYC", ZipCode = "10001" };

        Assert.False(address.Equals(null));
    }

    [Fact]
    public void Equals_DifferentType_ShouldReturnFalse()
    {
        var address = new Address { Street = "Main St", City = "NYC", ZipCode = "10001" };
        var money = new Money { Amount = 100, Currency = "USD" };

        Assert.False(address.Equals(money));
    }

    [Fact]
    public void Equals_DifferentTypeButSameComponent_ShouldReturnFalse()
    {
        var address = new Address { Street = "Main St", City = "NYC", ZipCode = "10001" };
        var addressLike = new AddressLike { Street = "Main St", City = "NYC", ZipCode = "10001" };

        Assert.False(address.Equals(addressLike));
    }

    [Fact]
    public void Equals_SameValues_ShouldReturnTrue()
    {
        var address1 = new Address { Street = "Main St", City = "NYC", ZipCode = "10001" };
        var address2 = new Address { Street = "Main St", City = "NYC", ZipCode = "10001" };

        Assert.True(address1.Equals(address2));
    }

    [Fact]
    public void Equals_DifferentValues_ShouldReturnFalse()
    {
        var address1 = new Address { Street = "Main St", City = "NYC", ZipCode = "10001" };
        var address2 = new Address { Street = "Broadway", City = "NYC", ZipCode = "10001" };

        Assert.False(address1.Equals(address2));
    }

    [Fact]
    public void Equals_OneComponentDifferent_ShouldReturnFalse()
    {
        var address1 = new Address { Street = "Main St", City = "NYC", ZipCode = "10001" };
        var address2 = new Address { Street = "Main St", City = "NYC", ZipCode = "10002" };

        Assert.False(address1.Equals(address2));
    }

    [Fact]
    public void Equals_EmptyValueObjects_ShouldReturnTrue()
    {
        var empty1 = new EmptyValueObject();
        var empty2 = new EmptyValueObject();

        Assert.True(empty1.Equals(empty2));
    }

    #endregion

    #region Operators

    [Fact]
    public void EqualityOperator_BothNull_ShouldReturnTrue()
    {
        Address? left = null;
        Address? right = null;

        Assert.True(left == right);
    }

    [Fact]
    public void EqualityOperator_SameReference_ShouldReturnTrue()
    {
        var address = new Address { Street = "Main St", City = "NYC", ZipCode = "10001" };
        Address left = address;
        Address right = address;

        Assert.True(left == right);
    }

    [Fact]
    public void EqualityOperator_LeftNull_ShouldReturnFalse()
    {
        Address? left = null;
        var right = new Address { Street = "Main St", City = "NYC", ZipCode = "10001" };

        Assert.False(left == right);
    }

    [Fact]
    public void EqualityOperator_RightNull_ShouldReturnFalse()
    {
        var left = new Address { Street = "Main St", City = "NYC", ZipCode = "10001" };
        Address? right = null;

        Assert.False(left == right);
    }

    [Fact]
    public void EqualityOperator_SameValues_ShouldReturnTrue()
    {
        var left = new Address { Street = "Main St", City = "NYC", ZipCode = "10001" };
        var right = new Address { Street = "Main St", City = "NYC", ZipCode = "10001" };

        Assert.True(left == right);
    }

    [Fact]
    public void EqualityOperator_DifferentValues_ShouldReturnFalse()
    {
        var left = new Address { Street = "Main St", City = "NYC", ZipCode = "10001" };
        var right = new Address { Street = "Broadway", City = "NYC", ZipCode = "10001" };

        Assert.False(left == right);
    }

    [Fact]
    public void InequalityOperator_SameValues_ShouldReturnFalse()
    {
        var left = new Address { Street = "Main St", City = "NYC", ZipCode = "10001" };
        var right = new Address { Street = "Main St", City = "NYC", ZipCode = "10001" };

        Assert.False(left != right);
    }

    [Fact]
    public void InequalityOperator_DifferentValues_ShouldReturnTrue()
    {
        var left = new Address { Street = "Main St", City = "NYC", ZipCode = "10001" };
        var right = new Address { Street = "Broadway", City = "NYC", ZipCode = "10001" };

        Assert.True(left != right);
    }

    #endregion

    #region GetHashCode

    [Fact]
    public void GetHashCode_SameValues_ShouldReturnSameHash()
    {
        var address1 = new Address { Street = "Main St", City = "NYC", ZipCode = "10001" };
        var address2 = new Address { Street = "Main St", City = "NYC", ZipCode = "10001" };

        Assert.Equal(address1.GetHashCode(), address2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentValues_ShouldReturnDifferentHash()
    {
        var address1 = new Address { Street = "Main St", City = "NYC", ZipCode = "10001" };
        var address2 = new Address { Street = "Broadway", City = "NYC", ZipCode = "10001" };

        Assert.NotEqual(address1.GetHashCode(), address2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_EmptyValueObject_ShouldNotThrow()
    {
        var empty = new EmptyValueObject();
        var hash = empty.GetHashCode();
    }

    #endregion

    #region ToString

    [Fact]
    public void ToString_ShouldReturnValidJson()
    {
        var address = new Address { Street = "Main St", City = "NYC", ZipCode = "10001" };

        var str = address.ToString();

        Assert.NotNull(str);
        Assert.NotEmpty(str);
    }

    [Fact]
    public void ToString_EmptyValueObject_ShouldReturnBraces()
    {
        var empty = new EmptyValueObject();

        var str = empty.ToString();

        Assert.Equal("{}", str);
    }

    [Fact]
    public void ToString_MoneyValueObject_ShouldReturnValidJson()
    {
        var money = new Money { Amount = 99.99m, Currency = "USD" };

        var str = money.ToString();

        Assert.NotNull(str);
        Assert.NotEmpty(str);
    }

    #endregion

    #region UseWithDictionary

    [Fact]
    public void CanUseAsDictionaryKey()
    {
        var dict = new Dictionary<Address, string>();
        var key = new Address { Street = "Main St", City = "NYC", ZipCode = "10001" };
        dict[key] = "value";

        var lookup = new Address { Street = "Main St", City = "NYC", ZipCode = "10001" };
        Assert.True(dict.ContainsKey(lookup));
        Assert.Equal("value", dict[lookup]);
    }

    [Fact]
    public void CanUseAsHashSet()
    {
        var set = new HashSet<Address>
        {
            new Address { Street = "Main St", City = "NYC", ZipCode = "10001" },
            new Address { Street = "Broadway", City = "NYC", ZipCode = "10001" }
        };

        Assert.Equal(2, set.Count);

        set.Add(new Address { Street = "Main St", City = "NYC", ZipCode = "10001" });
        Assert.Equal(2, set.Count);
    }

    #endregion
}
