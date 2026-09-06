using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;

namespace NoelleNet.AspNetCore.Security.Claims;

/// <summary>
/// <see cref="NoelleHttpContextCurrentPrincipalProvider"/> 的契约测试
/// </summary>
public class NoelleHttpContextCurrentPrincipalProviderTests
{
    /// <summary>
    /// HttpContext 为 null 时 Principal 应为 null（非 Web 场景空安全）
    /// </summary>
    [Fact]
    public void Principal_HttpContextIsNull_ShouldReturnNull()
    {
        var accessorMock = new Mock<IHttpContextAccessor>();
        accessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null);

        var provider = new NoelleHttpContextCurrentPrincipalProvider(accessorMock.Object);

        Assert.Null(provider.Principal);
    }

    /// <summary>
    /// 已认证用户应原样返回（含其声明）
    /// </summary>
    [Fact]
    public void Principal_HttpContextHasUser_ShouldReturnUser()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "123"), new Claim("role", "admin")], "test"));
        var accessorMock = new Mock<IHttpContextAccessor>();
        accessorMock.Setup(a => a.HttpContext).Returns(new DefaultHttpContext { User = user });

        var provider = new NoelleHttpContextCurrentPrincipalProvider(accessorMock.Object);

        Assert.Same(user, provider.Principal);
        Assert.True(provider.Principal!.HasClaim("sub", "123"));
    }

    /// <summary>
    /// 未显式设置 User 时，DefaultHttpContext 自动创建空 Principal（ICurrentUser 不空引用）
    /// </summary>
    [Fact]
    public void Principal_UserNotSet_ShouldReturnDefaultPrincipal()
    {
        var accessorMock = new Mock<IHttpContextAccessor>();
        accessorMock.Setup(a => a.HttpContext).Returns(new DefaultHttpContext());

        var provider = new NoelleHttpContextCurrentPrincipalProvider(accessorMock.Object);

        Assert.NotNull(provider.Principal);
    }
}
