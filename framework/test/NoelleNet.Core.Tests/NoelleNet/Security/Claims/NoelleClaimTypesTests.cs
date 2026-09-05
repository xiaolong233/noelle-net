namespace NoelleNet.Security.Claims;

public class NoelleClaimTypesTests
{
    [Fact]
    public void ClientId_Default_ShouldBeClientId()
    {
        Assert.Equal("client_id", NoelleClaimTypes.ClientId);
    }

    [Fact]
    public void Subject_Default_ShouldBeSub()
    {
        Assert.Equal("sub", NoelleClaimTypes.Subject);
    }

    [Fact]
    public void UserId_Default_ShouldBeUserId()
    {
        Assert.Equal("user_id", NoelleClaimTypes.UserId);
    }

    [Fact]
    public void UserName_Default_ShouldBePreferredUsername()
    {
        Assert.Equal("preferred_username", NoelleClaimTypes.UserName);
    }

    [Fact]
    public void Name_Default_ShouldBeName()
    {
        Assert.Equal("name", NoelleClaimTypes.Name);
    }

    [Fact]
    public void GivenName_Default_ShouldBeGivenName()
    {
        Assert.Equal("given_name", NoelleClaimTypes.GivenName);
    }

    [Fact]
    public void Surname_Default_ShouldBeFamilyName()
    {
        Assert.Equal("family_name", NoelleClaimTypes.Surname);
    }

    [Fact]
    public void MiddleName_Default_ShouldBeMiddleName()
    {
        Assert.Equal("middle_name", NoelleClaimTypes.MiddleName);
    }

    [Fact]
    public void NickName_Default_ShouldBeNickname()
    {
        Assert.Equal("nickname", NoelleClaimTypes.NickName);
    }

    [Fact]
    public void Email_Default_ShouldBeEmail()
    {
        Assert.Equal("email", NoelleClaimTypes.Email);
    }

    [Fact]
    public void EmailVerified_Default_ShouldBeEmailVerified()
    {
        Assert.Equal("email_verified", NoelleClaimTypes.EmailVerified);
    }

    [Fact]
    public void PhoneNumber_Default_ShouldBePhoneNumber()
    {
        Assert.Equal("phone_number", NoelleClaimTypes.PhoneNumber);
    }

    [Fact]
    public void PhoneNumberVerified_Default_ShouldBePhoneNumberVerified()
    {
        Assert.Equal("phone_number_verified", NoelleClaimTypes.PhoneNumberVerified);
    }

    [Fact]
    public void Gender_Default_ShouldBeGender()
    {
        Assert.Equal("gender", NoelleClaimTypes.Gender);
    }

    [Fact]
    public void DateOfBirth_Default_ShouldBeBirthdate()
    {
        Assert.Equal("birthdate", NoelleClaimTypes.DateOfBirth);
    }

    [Fact]
    public void Role_Default_ShouldBeRole()
    {
        Assert.Equal("role", NoelleClaimTypes.Role);
    }

    [Fact]
    public void Permission_Default_ShouldBePermission()
    {
        Assert.Equal("permission", NoelleClaimTypes.Permission);
    }

    [Fact]
    public void OrganizationUnitId_Default_ShouldBeOrganizationUnitId()
    {
        Assert.Equal("organization_unit_id", NoelleClaimTypes.OrganizationUnitId);
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        NoelleClaimTypes.ClientId = "my_client_id";
        Assert.Equal("my_client_id", NoelleClaimTypes.ClientId);
        // 重置
        NoelleClaimTypes.ClientId = "client_id";
    }
}
