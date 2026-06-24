using System.Security.Claims;
using Moq;
using NoelleNet.Security.Claims;

namespace NoelleNet.Security;

public class CurrentUserTests
{
    private readonly Mock<ICurrentPrincipalProvider> _providerMock;
    private readonly ClaimsPrincipal _principal;

    public CurrentUserTests()
    {
        _providerMock = new Mock<ICurrentPrincipalProvider>();
        _principal = new ClaimsPrincipal();
        _providerMock.Setup(p => p.Principal).Returns(_principal);
    }

    [Fact]
    public void Constructor_NullProvider_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new CurrentUser(null!));
    }

    [Fact]
    public void ShouldImplementICurrentUser()
    {
        var user = new CurrentUser(_providerMock.Object);
        Assert.IsAssignableFrom<ICurrentUser>(user);
    }

    [Fact]
    public void Id_WithClaim_ShouldReturnValue()
    {
        _principal.AddIdentity(new ClaimsIdentity(new[]
        {
            new Claim(NoelleClaimTypes.UserId, "user123")
        }));

        var user = new CurrentUser(_providerMock.Object);
        Assert.Equal("user123", user.Id);
    }

    [Fact]
    public void Id_WithoutClaim_ShouldReturnNull()
    {
        var user = new CurrentUser(_providerMock.Object);
        Assert.Null(user.Id);
    }

    [Fact]
    public void UserName_WithClaim_ShouldReturnValue()
    {
        _principal.AddIdentity(new ClaimsIdentity(new[]
        {
            new Claim(NoelleClaimTypes.UserName, "zhangsan")
        }));

        var user = new CurrentUser(_providerMock.Object);
        Assert.Equal("zhangsan", user.UserName);
    }

    [Fact]
    public void Email_WithClaim_ShouldReturnValue()
    {
        _principal.AddIdentity(new ClaimsIdentity(new[]
        {
            new Claim(NoelleClaimTypes.Email, "test@example.com")
        }));

        var user = new CurrentUser(_providerMock.Object);
        Assert.Equal("test@example.com", user.Email);
    }

    [Fact]
    public void EmailConfirmed_TrueClaim_ShouldReturnTrue()
    {
        _principal.AddIdentity(new ClaimsIdentity(new[]
        {
            new Claim(NoelleClaimTypes.EmailVerified, "true")
        }));

        var user = new CurrentUser(_providerMock.Object);
        Assert.True(user.EmailConfirmed);
    }

    [Fact]
    public void EmailConfirmed_FalseClaim_ShouldReturnFalse()
    {
        _principal.AddIdentity(new ClaimsIdentity(new[]
        {
            new Claim(NoelleClaimTypes.EmailVerified, "false")
        }));

        var user = new CurrentUser(_providerMock.Object);
        Assert.False(user.EmailConfirmed);
    }

    [Fact]
    public void EmailConfirmed_NoClaim_ShouldReturnFalse()
    {
        var user = new CurrentUser(_providerMock.Object);
        Assert.False(user.EmailConfirmed);
    }

    [Fact]
    public void PhoneNumberConfirmed_TrueClaim_ShouldReturnTrue()
    {
        _principal.AddIdentity(new ClaimsIdentity(new[]
        {
            new Claim(NoelleClaimTypes.PhoneNumberVerified, "TRUE")
        }));

        var user = new CurrentUser(_providerMock.Object);
        Assert.True(user.PhoneNumberConfirmed);
    }

    [Fact]
    public void DateOfBirth_WithValidDate_ShouldReturnDate()
    {
        _principal.AddIdentity(new ClaimsIdentity(new[]
        {
            new Claim(NoelleClaimTypes.DateOfBirth, "1990-01-01")
        }));

        var user = new CurrentUser(_providerMock.Object);
        Assert.Equal(new DateTime(1990, 1, 1), user.DateOfBirth);
    }

    [Fact]
    public void DateOfBirth_WithInvalidDate_ShouldReturnNull()
    {
        _principal.AddIdentity(new ClaimsIdentity(new[]
        {
            new Claim(NoelleClaimTypes.DateOfBirth, "not-a-date")
        }));

        var user = new CurrentUser(_providerMock.Object);
        Assert.Null(user.DateOfBirth);
    }

    [Fact]
    public void Roles_WithRoles_ShouldReturnAll()
    {
        _principal.AddIdentity(new ClaimsIdentity(new[]
        {
            new Claim(NoelleClaimTypes.Role, "admin"),
            new Claim(NoelleClaimTypes.Role, "user")
        }));

        var user = new CurrentUser(_providerMock.Object);
        Assert.Equal(new[] { "admin", "user" }, user.Roles);
    }

    [Fact]
    public void IsInRole_ExistingRole_ShouldReturnTrue()
    {
        _principal.AddIdentity(new ClaimsIdentity(new[]
        {
            new Claim(NoelleClaimTypes.Role, "admin")
        }));

        var user = new CurrentUser(_providerMock.Object);
        Assert.True(user.IsInRole("admin"));
    }

    [Fact]
    public void IsInRole_NonExistingRole_ShouldReturnFalse()
    {
        var user = new CurrentUser(_providerMock.Object);
        Assert.False(user.IsInRole("admin"));
    }

    [Fact]
    public void Permissions_WithPermissions_ShouldReturnAll()
    {
        _principal.AddIdentity(new ClaimsIdentity(new[]
        {
            new Claim(NoelleClaimTypes.Permission, "read"),
            new Claim(NoelleClaimTypes.Permission, "write")
        }));

        var user = new CurrentUser(_providerMock.Object);
        Assert.Equal(new[] { "read", "write" }, user.Permissions);
    }

    [Fact]
    public void HasPermission_ExistingPermission_ShouldReturnTrue()
    {
        _principal.AddIdentity(new ClaimsIdentity(new[]
        {
            new Claim(NoelleClaimTypes.Permission, "read")
        }));

        var user = new CurrentUser(_providerMock.Object);
        Assert.True(user.HasPermission("read"));
    }

    [Fact]
    public void HasPermission_NonExistingPermission_ShouldReturnFalse()
    {
        var user = new CurrentUser(_providerMock.Object);
        Assert.False(user.HasPermission("read"));
    }

    [Fact]
    public void Claims_ShouldReturnAllClaims()
    {
        _principal.AddIdentity(new ClaimsIdentity(new[]
        {
            new Claim("type1", "value1"),
            new Claim("type2", "value2")
        }));

        var user = new CurrentUser(_providerMock.Object);
        Assert.Equal(2, user.Claims.Length);
    }

    [Fact]
    public void Claims_NoPrincipal_ShouldReturnEmpty()
    {
        _providerMock.Setup(p => p.Principal).Returns((ClaimsPrincipal?)null);
        var user = new CurrentUser(_providerMock.Object);
        Assert.Empty(user.Claims);
    }

    [Fact]
    public void FindClaim_ExistingType_ShouldReturnClaim()
    {
        _principal.AddIdentity(new ClaimsIdentity(new[]
        {
            new Claim("custom", "value")
        }));

        var user = new CurrentUser(_providerMock.Object);
        var claim = user.FindClaim("custom");
        Assert.NotNull(claim);
        Assert.Equal("value", claim!.Value);
    }

    [Fact]
    public void FindClaim_NoPrincipal_ShouldReturnNull()
    {
        _providerMock.Setup(p => p.Principal).Returns((ClaimsPrincipal?)null);
        var user = new CurrentUser(_providerMock.Object);
        Assert.Null(user.FindClaim("any"));
    }

    [Fact]
    public void FindClaimValue_ExistingType_ShouldReturnValue()
    {
        _principal.AddIdentity(new ClaimsIdentity(new[]
        {
            new Claim("custom", "myvalue")
        }));

        var user = new CurrentUser(_providerMock.Object);
        Assert.Equal("myvalue", user.FindClaimValue("custom"));
    }

    [Fact]
    public void FindClaimValue_NoPrincipal_ShouldReturnNull()
    {
        _providerMock.Setup(p => p.Principal).Returns((ClaimsPrincipal?)null);
        var user = new CurrentUser(_providerMock.Object);
        Assert.Null(user.FindClaimValue("any"));
    }

    [Fact]
    public void FindClaims_MultipleMatches_ShouldReturnAll()
    {
        _principal.AddIdentity(new ClaimsIdentity(new[]
        {
            new Claim("role", "admin"),
            new Claim("role", "user")
        }));

        var user = new CurrentUser(_providerMock.Object);
        Assert.Equal(2, user.FindClaims("role").Length);
    }

    [Fact]
    public void FindClaims_NoPrincipal_ShouldReturnEmpty()
    {
        _providerMock.Setup(p => p.Principal).Returns((ClaimsPrincipal?)null);
        var user = new CurrentUser(_providerMock.Object);
        Assert.Empty(user.FindClaims("any"));
    }

    [Fact]
    public void FindClaimValues_ShouldReturnAllValues()
    {
        _principal.AddIdentity(new ClaimsIdentity(new[]
        {
            new Claim("role", "admin"),
            new Claim("role", "user")
        }));

        var user = new CurrentUser(_providerMock.Object);
        Assert.Equal(new[] { "admin", "user" }, user.FindClaimValues("role"));
    }

    [Fact]
    public void FindClaimValues_NoPrincipal_ShouldReturnEmpty()
    {
        _providerMock.Setup(p => p.Principal).Returns((ClaimsPrincipal?)null);
        var user = new CurrentUser(_providerMock.Object);
        Assert.Empty(user.FindClaimValues("any"));
    }

    [Fact]
    public void AllProperties_WithClaims_ShouldReturnValues()
    {
        _principal.AddIdentity(new ClaimsIdentity(new[]
        {
            new Claim(NoelleClaimTypes.ClientId, "client1"),
            new Claim(NoelleClaimTypes.DeptId, "dept1"),
            new Claim(NoelleClaimTypes.GivenName, "San"),
            new Claim(NoelleClaimTypes.Surname, "Zhang"),
            new Claim(NoelleClaimTypes.NickName, "xiaozhang"),
            new Claim(NoelleClaimTypes.PhoneNumber, "13800138000"),
            new Claim(NoelleClaimTypes.Gender, "male")
        }));

        var user = new CurrentUser(_providerMock.Object);
        Assert.Equal("client1", user.ClientId);
        Assert.Equal("dept1", user.DeptId);
        Assert.Equal("San", user.GivenName);
        Assert.Equal("Zhang", user.Surname);
        Assert.Equal("xiaozhang", user.NickName);
        Assert.Equal("13800138000", user.PhoneNumber);
        Assert.Equal("male", user.Gender);
    }
}
