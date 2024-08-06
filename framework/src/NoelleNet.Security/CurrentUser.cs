using NoelleNet.Security.Claims;
using System.Security.Claims;
using System.Text.Json;

namespace NoelleNet.Security;

/// <summary>
/// 当前用户信息 <see cref="ICurrentUser"/> 的默认实现
/// </summary>
public class CurrentUser : ICurrentUser
{
    private readonly ICurrentPrincipalProvider _principalProvider;

    public CurrentUser(ICurrentPrincipalProvider principalProvider)
    {
        _principalProvider = principalProvider;
    }

    public virtual string? ClientId => FindClaimValue(NoelleClaimTypes.ClientId);

    public virtual string? Id => FindClaimValue(NoelleClaimTypes.Subject);

    public virtual string? UserName => FindClaimValue(NoelleClaimTypes.UserName);

    public virtual string? Name => FindClaimValue(NoelleClaimTypes.Name);

    public virtual string? FirstName => FindClaimValue(NoelleClaimTypes.GivenName);

    public virtual string? LastName => FindClaimValue(NoelleClaimTypes.FamilyName);

    public virtual string? MiddleName => FindClaimValue(NoelleClaimTypes.MiddleName);

    public virtual string? NickName => FindClaimValue(NoelleClaimTypes.NickName);

    public virtual string? Email => FindClaimValue(NoelleClaimTypes.Email);

    public virtual bool EmailConfirmed => FindClaimValue__Boolean(NoelleClaimTypes.EmailVerified);

    public virtual string? PhoneNumber => FindClaimValue(NoelleClaimTypes.PhoneNumber);

    public virtual bool PhoneNumberConfirmed => FindClaimValue__Boolean(NoelleClaimTypes.PhoneNumberVerified);

    public virtual string? Gender => FindClaimValue(NoelleClaimTypes.Gender);

    public virtual string? DateOfBirth => FindClaimValue(NoelleClaimTypes.BirthDate);

    public virtual string[] Roles => FindClaimValue__Array(NoelleClaimTypes.Roles);

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

    private bool FindClaimValue__Boolean(string claimType)
    {
        return string.Equals(FindClaimValue(claimType), "true", StringComparison.InvariantCultureIgnoreCase);
    }

    private string[] FindClaimValue__Array(string claimType)
    {
        string? value = FindClaimValue(claimType);
        if (string.IsNullOrWhiteSpace(value))
            return [];

        return JsonSerializer.Deserialize<string[]>(value) ?? [];
    }

    public virtual Claim[] GetAllClaims()
    {
        return _principalProvider.Principal?.Claims.ToArray() ?? [];
    }

    public virtual bool IsInRole(string roleName)
    {
        return Roles.Any(r => r == roleName);
    }
}
