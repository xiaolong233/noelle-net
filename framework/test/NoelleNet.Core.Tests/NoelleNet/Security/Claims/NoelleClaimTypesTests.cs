using System.Security.Claims;

namespace NoelleNet.Security.Claims;

public class NoelleClaimTypesTests
{
    [Fact]
    public void ClientId_Default_ShouldBeClientId()
    {
        Assert.Equal("client_id", NoelleClaimTypes.ClientId);
    }

    [Fact]
    public void UserId_Default_ShouldBeNameIdentifier()
    {
        Assert.Equal(ClaimTypes.NameIdentifier, NoelleClaimTypes.UserId);
    }

    [Fact]
    public void UserName_Default_ShouldBeName()
    {
        Assert.Equal(ClaimTypes.Name, NoelleClaimTypes.UserName);
    }

    [Fact]
    public void GivenName_Default_ShouldBeGivenName()
    {
        Assert.Equal(ClaimTypes.GivenName, NoelleClaimTypes.GivenName);
    }

    [Fact]
    public void Surname_Default_ShouldBeSurname()
    {
        Assert.Equal(ClaimTypes.Surname, NoelleClaimTypes.Surname);
    }

    [Fact]
    public void NickName_Default_ShouldBeNickname()
    {
        Assert.Equal("nickname", NoelleClaimTypes.NickName);
    }

    [Fact]
    public void Email_Default_ShouldBeEmail()
    {
        Assert.Equal(ClaimTypes.Email, NoelleClaimTypes.Email);
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
        Assert.Equal(ClaimTypes.Gender, NoelleClaimTypes.Gender);
    }

    [Fact]
    public void DateOfBirth_Default_ShouldBeDateOfBirth()
    {
        Assert.Equal(ClaimTypes.DateOfBirth, NoelleClaimTypes.DateOfBirth);
    }

    [Fact]
    public void Role_Default_ShouldBeRole()
    {
        Assert.Equal(ClaimTypes.Role, NoelleClaimTypes.Role);
    }

    [Fact]
    public void Permission_Default_ShouldBePermission()
    {
        Assert.Equal("permission", NoelleClaimTypes.Permission);
    }

    [Fact]
    public void DeptId_Default_ShouldBeDeptId()
    {
        Assert.Equal("dept_id", NoelleClaimTypes.DeptId);
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
