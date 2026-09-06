namespace NoelleNet.Ddd.Domain.Entities;

#region 测试值对象

/// <summary>
/// 地址值对象
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
/// 与 Address 组件相同但类型不同的值对象（验证类型参与相等性）
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

/// <summary>
/// 无相等组件的值对象
/// </summary>
internal class EmptyValueObject : ValueObject
{
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield break;
    }
}

#endregion

/// <summary>
/// <see cref="ValueObject"/> 的契约测试：值相等性、运算符、哈希与集合语义
/// </summary>
public class ValueObjectTests
{
    private static Address CreateAddress(string street = "Main St", string city = "NYC", string zip = "10001")
        => new() { Street = street, City = city, ZipCode = zip };

    #region Equals

    /// <summary>
    /// 同引用相等；与 null 不相等；不同类型即使组件相同也不相等
    /// </summary>
    [Fact]
    public void Equals_ReferenceAndTypeSemantics_ShouldHold()
    {
        var address = CreateAddress();

        Assert.True(address.Equals(address));
        Assert.False(address.Equals(null));
        Assert.False(address.Equals(new AddressLike { Street = "Main St", City = "NYC", ZipCode = "10001" }));
    }

    /// <summary>
    /// 组件全等则相等，任一组件不同则不相等
    /// </summary>
    [Fact]
    public void Equals_ShouldCompareByComponents()
    {
        Assert.True(CreateAddress().Equals(CreateAddress()));
        Assert.False(CreateAddress().Equals(CreateAddress(street: "Broadway")));
        Assert.False(CreateAddress().Equals(CreateAddress(zip: "10002")));
    }

    /// <summary>
    /// 无相等组件的两个实例相等（空序列相等）
    /// </summary>
    [Fact]
    public void Equals_EmptyValueObjects_ShouldReturnTrue()
    {
        Assert.True(new EmptyValueObject().Equals(new EmptyValueObject()));
    }

    #endregion

    #region Operators

    /// <summary>
    /// == 与 != 运算符：null 情况与组件比较
    /// </summary>
    [Fact]
    public void EqualityOperators_ShouldHandleNullsAndComponents()
    {
        Address? left = null;
        Address? right = null;
        Assert.True(left == right);
        Assert.False(left == CreateAddress());
        Assert.False(CreateAddress() == null);

        Assert.True(CreateAddress() == CreateAddress());
        Assert.False(CreateAddress() == CreateAddress(street: "Broadway"));
        Assert.False(CreateAddress() != CreateAddress());
        Assert.True(CreateAddress() != CreateAddress(street: "Broadway"));
    }

    #endregion

    #region GetHashCode

    /// <summary>
    /// 相同组件哈希相同、不同组件哈希不同；空值对象不抛异常
    /// </summary>
    [Fact]
    public void GetHashCode_ShouldFollowComponents()
    {
        Assert.Equal(CreateAddress().GetHashCode(), CreateAddress().GetHashCode());
        Assert.NotEqual(CreateAddress().GetHashCode(), CreateAddress(street: "Broadway").GetHashCode());
        Assert.NotNull(new EmptyValueObject().GetHashCode());
    }

    #endregion

    #region ToString

    /// <summary>
    /// ToString 返回 JSON；空值对象输出 "{}"
    /// </summary>
    [Fact]
    public void ToString_ShouldReturnJson()
    {
        Assert.Equal("{}", new EmptyValueObject().ToString());

        var json = CreateAddress().ToString();
        Assert.Contains("Main St", json);
        Assert.Contains("NYC", json);
    }

    #endregion

    #region 集合语义

    /// <summary>
    /// 值对象可作为字典键：同值不同实例应命中
    /// </summary>
    [Fact]
    public void CanUseAsDictionaryKey()
    {
        var dict = new Dictionary<Address, string> { [CreateAddress()] = "value" };

        Assert.True(dict.ContainsKey(CreateAddress()));
        Assert.Equal("value", dict[CreateAddress()]);
    }

    /// <summary>
    /// 值对象在 HashSet 中按值去重
    /// </summary>
    [Fact]
    public void CanUseAsHashSet()
    {
        var set = new HashSet<Address> { CreateAddress(), CreateAddress(street: "Broadway") };

        set.Add(CreateAddress());

        Assert.Equal(2, set.Count);
    }

    #endregion
}
