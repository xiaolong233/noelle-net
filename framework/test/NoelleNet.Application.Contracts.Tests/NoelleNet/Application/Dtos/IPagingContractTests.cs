#pragma warning disable CS0618 // 本测试有意引用 [Obsolete] 的旧接口以锁定兼容契约

namespace NoelleNet.Application.Dtos;

/// <summary>
/// 分页接口形状的契约测试：锁定 Paging/Paged 词形方案下的接口组合关系，
/// 防止未来改动（如把 Sort 从 IPaging 中拆出）在无感知的情况下破坏契约。
/// </summary>
public class IPagingContractTests
{
    /// <summary>
    /// IPaging 应同时继承 IHasLimit 与 IHasSort（Sort 已并入分页契约）
    /// </summary>
    [Fact]
    public void IPaging_ShouldCombineIHasLimitAndIHasSort()
    {
        Assert.True(typeof(IHasLimit).IsAssignableFrom(typeof(IPaging)));
        Assert.True(typeof(IHasSort).IsAssignableFrom(typeof(IPaging)));
    }

    /// <summary>
    /// IPagedResult 应同时继承 IListResult 与 IHasTotalCount
    /// </summary>
    [Fact]
    public void IPagedResult_ShouldCombineIListResultAndIHasTotalCount()
    {
        Assert.True(typeof(IListResult<object>).IsAssignableFrom(typeof(IPagedResult<object>)));
        Assert.True(typeof(IHasTotalCount).IsAssignableFrom(typeof(IPagedResult<object>)));
    }

    /// <summary>
    /// 旧类型 IPagination 系列应保持可用（冻结兼容），且与 IPaging 无继承耦合
    /// </summary>
    [Fact]
    public void LegacyInterfaces_ShouldRemainIndependentOfNewOnes()
    {
        // 旧接口保持原形状：IPagination 只含 IHasLimit，不包含 Sort
        Assert.True(typeof(IHasLimit).IsAssignableFrom(typeof(IPagination)));
        Assert.False(typeof(IHasSort).IsAssignableFrom(typeof(IPagination)));

        // 新接口与旧接口彼此独立（旧类型冻结，不做继承 shim）
        Assert.False(typeof(IPaging).IsAssignableFrom(typeof(IPagination)));
        Assert.False(typeof(IPagination).IsAssignableFrom(typeof(IPaging)));
    }
}
