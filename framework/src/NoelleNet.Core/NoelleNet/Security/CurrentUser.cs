using NoelleNet.Security.Claims;
using System.Security.Claims;

namespace NoelleNet.Security;

/// <summary>
/// 当前用户信息 <see cref="ICurrentUser"/> 的默认实现
/// </summary>
public class CurrentUser(ICurrentPrincipalProvider principalProvider) : ICurrentUser
{
    private readonly ICurrentPrincipalProvider _principalProvider = principalProvider;

    public virtual string? ClientId => FindClaimValue(NoelleClaimTypes.ClientId);

    public virtual string? Id => FindClaimValue(NoelleClaimTypes.UserId);

    public virtual string? UserName => FindClaimValue(NoelleClaimTypes.UserName);

    public virtual string? NickName => FindClaimValue(NoelleClaimTypes.NickName);

    public virtual string? Email => FindClaimValue(NoelleClaimTypes.Email);

    public virtual bool EmailConfirmed => FindClaimBooleanValue(NoelleClaimTypes.EmailVerified);

    public virtual string? PhoneNumber => FindClaimValue(NoelleClaimTypes.PhoneNumber);

    public virtual bool PhoneNumberConfirmed => FindClaimBooleanValue(NoelleClaimTypes.PhoneNumberVerified);

    public virtual string? Gender => FindClaimValue(NoelleClaimTypes.Gender);

    public virtual DateTime? DateOfBirth => FindClaimDateTimeValue(NoelleClaimTypes.DateOfBirth);

    public virtual string[] Roles => FindClaimValues(NoelleClaimTypes.Role);

    public string? DeptId => FindClaimValue(NoelleClaimTypes.DeptId);

    public string? GivenName => FindClaimValue(NoelleClaimTypes.GivenName);

    public string? Surname => FindClaimValue(NoelleClaimTypes.Surname);

    public string[] Permissions => FindClaimValues(NoelleClaimTypes.Permission);

    public string? DataAccessScope => FindClaimValue(NoelleClaimTypes.DataAccessScope);

    public string[] AccessibleDepts => FindClaimValues(NoelleClaimTypes.AccessibleDept);

    public virtual Claim? FindClaim(string claimType)
    {
        return _principalProvider.Principal?.Claims.FirstOrDefault(c => c.Type == claimType);
    }

    public virtual Claim[] FindClaims(string claimType)
    {
        return _principalProvider.Principal?.Claims.Where(c => c.Type == claimType).ToArray() ?? [];
    }

    public virtual string? FindClaimValue(string claimType)
    {
        return FindClaim(claimType)?.Value;
    }

    public virtual Claim[] GetAllClaims()
    {
        return _principalProvider.Principal?.Claims.ToArray() ?? [];
    }

    public virtual bool IsInRole(string roleName)
    {
        return Roles.Any(r => r == roleName);
    }

    public bool HasPermission(string permission)
    {
        return Permissions.Any(p => p == permission);
    }

    public string[] FindClaimValues(string claimType)
    {
        return FindClaims(claimType).Select(c => c.Value).ToArray();
    }

    private bool FindClaimBooleanValue(string claimType)
    {
        return string.Equals(FindClaimValue(claimType), "true", StringComparison.InvariantCultureIgnoreCase);
    }

    private DateTime? FindClaimDateTimeValue(string claimType)
    {
        return DateTime.TryParse(FindClaimValue(claimType), out DateTime value) ? value : null;
    }
}
