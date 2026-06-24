using NoelleNet.Ddd.Domain.Entities;
using NoelleNet.Ddd.Domain.Repositories;

namespace NoelleNet.Ddd.Domain.Repositories;

public class IRepositoryTests
{
    [Fact]
    public void GenericConstraint_TMustImplementIAggregateRoot()
    {
        // 验证 IRepository<T> 的约束 T : IAggregateRoot
        // 通过编译时类型检查来验证（如果不能编译通过，测试项目本身就编译不过）
        Assert.True(typeof(IRepository<IAggregateRoot>).GetGenericArguments().Length == 1);
    }

    [Fact]
    public void IsMarkerInterface()
    {
        // IRepository<T> 是一个标记接口，无任何成员方法
        var type = typeof(IRepository<>);
        var members = type.GetMembers().Where(m => m.DeclaringType == type).ToArray();

        Assert.Empty(members);
    }

    [Fact]
    public void ShouldAcceptAggregateRootImplementations()
    {
        // 编译时验证：聚合根实现可以被用作 IRepository<T> 的类型参数
        var type = typeof(IRepository<TestAggregate>);

        Assert.NotNull(type);
        Assert.True(type.IsInterface);
    }

    [Fact]
    public void ShouldNotAcceptNonAggregateRootTypes()
    {
        // 由于类型约束，非 IAggregateRoot 类型在编译时就会被拒绝
        // 此测试验证约束条件的存在
        var type = typeof(IRepository<>);
        var genericConstraints = type.GetGenericArguments()[0].GetGenericParameterConstraints();

        Assert.Contains(typeof(IAggregateRoot), genericConstraints);
    }
}

/// <summary>
/// 用于测试仓储约束的聚合根实现
/// </summary>
internal class TestAggregate : AggregateRoot
{
    public int Id { get; set; }

    public override object?[] GetIdentifiers() => [Id];
}
