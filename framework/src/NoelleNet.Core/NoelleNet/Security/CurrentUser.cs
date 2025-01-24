using NoelleNet.Security.Claims;
using System.Security.Claims;

namespace NoelleNet.Security;

/// <summary>
/// <see cref="ICurrentUser"/> 的默认实现
/// </summary>
public class CurrentUser(ICurrentPrincipalProvider principalProvider) : ICurrentUser
{
    private readonly ICurrentPrincipalProvider _principalProvider = principalProvider;

    public string? ClientId => FindClaimValue(NoelleClaimTypes.ClientId);

    public string? Id => FindClaimValue(NoelleClaimTypes.UserId);

    public string? DeptId => FindClaimValue(NoelleClaimTypes.DeptId);

    public string? UserName => FindClaimValue(NoelleClaimTypes.UserName);

    public string? GivenName => FindClaimValue(NoelleClaimTypes.GivenName);

    public string? Surname => FindClaimValue(NoelleClaimTypes.Surname);

    public string? NickName => FindClaimValue(NoelleClaimTypes.NickName);

    public string? Email => FindClaimValue(NoelleClaimTypes.Email);

    public bool EmailConfirmed => FindClaimBooleanValue(NoelleClaimTypes.EmailVerified);

    public string? PhoneNumber => FindClaimValue(NoelleClaimTypes.PhoneNumber);

    public bool PhoneNumberConfirmed => FindClaimBooleanValue(NoelleClaimTypes.PhoneNumberVerified);

    public string? Gender => FindClaimValue(NoelleClaimTypes.Gender);

    public DateTime? DateOfBirth => FindClaimDateTimeValue(NoelleClaimTypes.DateOfBirth);

    public string[] Roles => FindClaimValues(NoelleClaimTypes.Role);

    public string[] Permissions => FindClaimValues(NoelleClaimTypes.Permission);

    public Claim[] Claims => _principalProvider.Principal?.Claims.ToArray() ?? [];

    public Claim? FindClaim(string claimType)
    {
        return _principalProvider.Principal?.FindFirst(claimType);
    }

    public Claim[] FindClaims(string claimType)
    {
        return _principalProvider.Principal?.FindAll(claimType).ToArray() ?? [];
    }

    public string? FindClaimValue(string claimType)
    {
        return _principalProvider.Principal?.FindFirst(claimType)?.Value;
    }

    public string[] FindClaimValues(string claimType)
    {
        return _principalProvider.Principal?.FindAll(claimType).Select(c => c.Value).ToArray() ?? [];
    }

    public bool HasPermission(string permission)
    {
        return Permissions.Any(p => p == permission);
    }

    public bool IsInRole(string roleName)
    {
        return Roles.Any(r => r == roleName);
    }

    #region 私有方法
    private bool FindClaimBooleanValue(string claimType)
    {
        return string.Equals(FindClaimValue(claimType), "true", StringComparison.InvariantCultureIgnoreCase);
    }

    private DateTime? FindClaimDateTimeValue(string claimType)
    {
        return DateTime.TryParse(FindClaimValue(claimType), out DateTime value) ? value : null;
    }
    #endregion
}
