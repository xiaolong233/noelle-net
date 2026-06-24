using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Moq;
using NoelleNet.Security.Claims;
using System.Security.Claims;

namespace NoelleNet.AspNetCore.Security.Claims;

public class NoelleHttpContextCurrentPrincipalProviderTests
{
    [Fact]
    public void Constructor_NullContextAccessor_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new NoelleHttpContextCurrentPrincipalProvider(null!));
    }

    [Fact]
    public void Principal_HttpContextIsNull_ShouldReturnNull()
    {
        var accessorMock = new Mock<IHttpContextAccessor>();
        accessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null);

        var provider = new NoelleHttpContextCurrentPrincipalProvider(accessorMock.Object);

        Assert.Null(provider.Principal);
    }

    [Fact]
    public void Principal_HttpContextHasUser_ShouldReturnUser()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim("name", "test")], "test"));
        var httpContext = new DefaultHttpContext { User = user };
        var accessorMock = new Mock<IHttpContextAccessor>();
        accessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        var provider = new NoelleHttpContextCurrentPrincipalProvider(accessorMock.Object);

        Assert.NotNull(provider.Principal);
        Assert.Same(user, provider.Principal);
    }

    [Fact]
    public void Principal_HttpContextUserNotSet_ShouldReturnDefaultPrincipal()
    {
        // When HttpContext.User has not been explicitly set, DefaultHttpContext
        // auto-creates a ClaimsPrincipal with an empty identity
        var httpContext = new DefaultHttpContext();
        var accessorMock = new Mock<IHttpContextAccessor>();
        accessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        var provider = new NoelleHttpContextCurrentPrincipalProvider(accessorMock.Object);

        // DefaultHttpContext auto-creates a ClaimsPrincipal even when not explicitly set
        Assert.NotNull(provider.Principal);
    }

    [Fact]
    public void Principal_UserHasClaims_ShouldContainClaims()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("sub", "123"), new Claim("role", "admin")], "test"));
        var httpContext = new DefaultHttpContext { User = user };
        var accessorMock = new Mock<IHttpContextAccessor>();
        accessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        var provider = new NoelleHttpContextCurrentPrincipalProvider(accessorMock.Object);

        Assert.True(provider.Principal!.HasClaim("sub", "123"));
        Assert.True(provider.Principal.HasClaim("role", "admin"));
    }
}
