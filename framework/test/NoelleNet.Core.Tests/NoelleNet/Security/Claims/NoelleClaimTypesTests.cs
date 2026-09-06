namespace NoelleNet.Security.Claims;

/// <summary>
/// <see cref="NoelleClaimTypes"/> 的契约测试：默认声明类型遵循 OpenID Connect 短名，
/// 这是 <see cref="Security.CurrentUser"/> 声明解析的基石，变更将破坏所有认证方案的声明映射。
/// </summary>
public class NoelleClaimTypesTests
{
    /// <summary>
    /// 全部默认值应遵循 OpenID Connect 标准短名
    /// </summary>
    [Fact]
    public void Defaults_ShouldFollowOpenIdConnectShortNames()
    {
        Assert.Equal("client_id", NoelleClaimTypes.ClientId);
        Assert.Equal("sub", NoelleClaimTypes.Subject);
        Assert.Equal("user_id", NoelleClaimTypes.UserId);
        Assert.Equal("preferred_username", NoelleClaimTypes.UserName);
        Assert.Equal("name", NoelleClaimTypes.Name);
        Assert.Equal("given_name", NoelleClaimTypes.GivenName);
        Assert.Equal("family_name", NoelleClaimTypes.Surname);
        Assert.Equal("middle_name", NoelleClaimTypes.MiddleName);
        Assert.Equal("nickname", NoelleClaimTypes.NickName);
        Assert.Equal("email", NoelleClaimTypes.Email);
        Assert.Equal("email_verified", NoelleClaimTypes.EmailVerified);
        Assert.Equal("phone_number", NoelleClaimTypes.PhoneNumber);
        Assert.Equal("phone_number_verified", NoelleClaimTypes.PhoneNumberVerified);
        Assert.Equal("gender", NoelleClaimTypes.Gender);
        Assert.Equal("birthdate", NoelleClaimTypes.DateOfBirth);
        Assert.Equal("role", NoelleClaimTypes.Role);
        Assert.Equal("permission", NoelleClaimTypes.Permission);
        Assert.Equal("organization_unit_id", NoelleClaimTypes.OrganizationUnitId);
    }

    /// <summary>
    /// 属性应可被覆盖（Cookie 认证等场景可替换为 ClaimTypes URI 类型）
    /// </summary>
    [Fact]
    public void Properties_ShouldBeSettable()
    {
        NoelleClaimTypes.ClientId = "custom_client_id";
        try
        {
            Assert.Equal("custom_client_id", NoelleClaimTypes.ClientId);
        }
        finally
        {
            NoelleClaimTypes.ClientId = "client_id";
        }
    }
}
