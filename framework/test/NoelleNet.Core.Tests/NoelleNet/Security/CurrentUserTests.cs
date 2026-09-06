using System.Security.Claims;
using Moq;
using NoelleNet.Security.Claims;

namespace NoelleNet.Security;

/// <summary>
/// <see cref="CurrentUser"/> 的单元测试：核心契约是"OpenID Connect 短名优先、ClaimTypes URI 回退"的声明解析策略
/// </summary>
public class CurrentUserTests
{
    private readonly Mock<ICurrentPrincipalProvider> _providerMock;

    public CurrentUserTests()
    {
        _providerMock = new Mock<ICurrentPrincipalProvider>();
    }

    private CurrentUser CreateUser(params Claim[] claims)
    {
        var principal = new ClaimsPrincipal();
        principal.AddIdentity(new ClaimsIdentity(claims));
        _providerMock.Setup(p => p.Principal).Returns(principal);
        return new CurrentUser(_providerMock.Object);
    }

    private CurrentUser CreateUserWithoutPrincipal()
    {
        _providerMock.Setup(p => p.Principal).Returns((ClaimsPrincipal?)null);
        return new CurrentUser(_providerMock.Object);
    }

    /// <summary>
    /// provider 为 null 时应抛出 ArgumentNullException
    /// </summary>
    [Fact]
    public void Constructor_NullProvider_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new CurrentUser(null!));
    }

    /// <summary>
    /// 同时存在短名与 URI 类型声明时，应优先返回短名值
    /// </summary>
    [Fact]
    public void UserName_WithBothClaimTypes_ShouldPreferOidcShortName()
    {
        var user = CreateUser(
            new Claim(NoelleClaimTypes.UserName, "preferred"),
            new Claim(ClaimTypes.Name, "uri-name"));

        Assert.Equal("preferred", user.UserName);
    }

    /// <summary>
    /// 仅有 URI 类型声明时（Cookie 认证场景），应回退到 ClaimTypes 值
    /// </summary>
    [Fact]
    public void Fallback_WithOnlyUriClaimTypes_ShouldReturnValues()
    {
        var user = CreateUser(
            new Claim(ClaimTypes.NameIdentifier, "id-1"),
            new Claim(ClaimTypes.Name, "zhangsan"),
            new Claim(ClaimTypes.Email, "test@example.com"));

        Assert.Equal("id-1", user.Subject);
        Assert.Equal("id-1", user.UserId);
        Assert.Equal("zhangsan", user.UserName);
        Assert.Equal("test@example.com", user.Email);
    }

    /// <summary>
    /// 无任何声明时，各属性应返回 null
    /// </summary>
    [Fact]
    public void Properties_WithoutClaims_ShouldReturnNull()
    {
        var user = CreateUser();

        Assert.Null(user.Subject);
        Assert.Null(user.UserId);
        Assert.Null(user.UserName);
        Assert.Null(user.Email);
        Assert.Null(user.PhoneNumber);
    }

    /// <summary>
    /// 布尔声明解析："true"（忽略大小写）为真，其余为假，无声明为假
    /// </summary>
    [Fact]
    public void ConfirmedProperties_ShouldParseBooleanClaims()
    {
        var user = CreateUser(
            new Claim(NoelleClaimTypes.EmailVerified, "true"),
            new Claim(NoelleClaimTypes.PhoneNumberVerified, "TRUE"));

        Assert.True(user.EmailConfirmed);
        Assert.True(user.PhoneNumberConfirmed);

        var user2 = CreateUser(new Claim(NoelleClaimTypes.EmailVerified, "false"));
        Assert.False(user2.EmailConfirmed);
        Assert.False(user2.PhoneNumberConfirmed);
    }

    /// <summary>
    /// 日期声明解析：合法日期返回 DateTime，非法或无声明返回 null
    /// </summary>
    [Fact]
    public void DateOfBirth_ShouldParseOrReturnNull()
    {
        var user = CreateUser(new Claim(NoelleClaimTypes.DateOfBirth, "1990-01-01"));
        Assert.Equal(new DateTime(1990, 1, 1), user.DateOfBirth);

        var user2 = CreateUser(new Claim(NoelleClaimTypes.DateOfBirth, "not-a-date"));
        Assert.Null(user2.DateOfBirth);
    }

    /// <summary>
    /// 角色：短名与 URI 类型取并集去重
    /// </summary>
    [Fact]
    public void Roles_ShouldReturnDistinctUnion()
    {
        var user = CreateUser(
            new Claim(NoelleClaimTypes.Role, "admin"),
            new Claim(ClaimTypes.Role, "admin"),
            new Claim(ClaimTypes.Role, "user"));

        Assert.Equal(["admin", "user"], user.Roles);
    }

    /// <summary>
    /// IsInRole 与 HasPermission 的判定
    /// </summary>
    [Fact]
    public void IsInRoleAndHasPermission_ShouldJudge()
    {
        var user = CreateUser(
            new Claim(NoelleClaimTypes.Role, "admin"),
            new Claim(NoelleClaimTypes.Permission, "read"));

        Assert.True(user.IsInRole("admin"));
        Assert.False(user.IsInRole("user"));
        Assert.True(user.HasPermission("read"));
        Assert.False(user.HasPermission("write"));
    }

    /// <summary>
    /// 声明查询 API：Claims/FindClaim/FindClaims/FindClaimValues
    /// </summary>
    [Fact]
    public void ClaimQueryApis_ShouldReturnClaims()
    {
        var user = CreateUser(
            new Claim("role", "admin"),
            new Claim("role", "user"),
            new Claim("custom", "value"));

        Assert.Equal(3, user.Claims.Length);
        Assert.Equal("value", user.FindClaim("custom")?.Value);
        Assert.Equal("value", user.FindClaimValue("custom"));
        Assert.Equal(2, user.FindClaims("role").Length);
        Assert.Equal(["admin", "user"], user.FindClaimValues("role"));
    }

    /// <summary>
    /// 无 Principal 时（未认证），所有查询 API 应返回空/默认值而不抛异常
    /// </summary>
    [Fact]
    public void ClaimQueryApis_WithoutPrincipal_ShouldReturnEmpty()
    {
        var user = CreateUserWithoutPrincipal();

        Assert.Empty(user.Claims);
        Assert.Empty(user.Roles);
        Assert.Empty(user.Permissions);
        Assert.Null(user.FindClaim("any"));
        Assert.Null(user.FindClaimValue("any"));
        Assert.Empty(user.FindClaims("any"));
        Assert.Empty(user.FindClaimValues("any"));
        Assert.False(user.IsInRole("admin"));
        Assert.False(user.HasPermission("read"));
    }

    /// <summary>
    /// 其余短名声明（ClientId/OrganizationUnitId/GivenName/Surname/MiddleName/NickName/PhoneNumber/Gender）应直接返回
    /// </summary>
    [Fact]
    public void OtherShortNameProperties_ShouldReturnValues()
    {
        var user = CreateUser(
            new Claim(NoelleClaimTypes.ClientId, "client1"),
            new Claim(NoelleClaimTypes.OrganizationUnitId, "dept1"),
            new Claim(NoelleClaimTypes.GivenName, "San"),
            new Claim(NoelleClaimTypes.Surname, "Zhang"),
            new Claim(NoelleClaimTypes.MiddleName, "M"),
            new Claim(NoelleClaimTypes.NickName, "xiaozhang"),
            new Claim(NoelleClaimTypes.PhoneNumber, "13800138000"),
            new Claim(NoelleClaimTypes.Gender, "male"));

        Assert.Equal("client1", user.ClientId);
        Assert.Equal("dept1", user.OrganizationUnitId);
        Assert.Equal("San", user.GivenName);
        Assert.Equal("Zhang", user.Surname);
        Assert.Equal("M", user.MiddleName);
        Assert.Equal("xiaozhang", user.NickName);
        Assert.Equal("13800138000", user.PhoneNumber);
        Assert.Equal("male", user.Gender);
    }
}
