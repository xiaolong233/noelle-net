using System.Security.Claims;

namespace NoelleNet.Security.Claims;

/// <summary>
/// <see cref="NoelleEmptyCurrentPrincipalProvider"/> 的单元测试
/// </summary>
public class NoelleEmptyCurrentPrincipalProviderTests
{
    /// <summary>
    /// 空实现应返回非空的空 ClaimsPrincipal，保证 ICurrentUser 在无认证场景下不抛空引用
    /// </summary>
    [Fact]
    public void Principal_ShouldReturnEmptyPrincipal()
    {
        var provider = new NoelleEmptyCurrentPrincipalProvider();

        Assert.NotNull(provider.Principal);
        Assert.Empty(provider.Principal.Claims);
    }
}
