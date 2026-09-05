using NoelleNet.Security.Claims;
using System.Security.Claims;

namespace NoelleNet.Security;

/// <summary>
/// <see cref="ICurrentUser"/> 的默认实现
/// 声明解析遵循"OpenID Connect 短名优先、<see cref="ClaimTypes"/> URI 类型回退"的策略，
/// 以兼容 Cookie 认证、JWT 承载认证及 OpenIddict 等不同认证方案产生的声明主体
/// </summary>
/// <param name="principalProvider"></param>
public class CurrentUser(ICurrentPrincipalProvider principalProvider) : ICurrentUser
{
    private readonly ICurrentPrincipalProvider _principalProvider = principalProvider ?? throw new ArgumentNullException(nameof(principalProvider));

    /// <inheritdoc/>
    public string? ClientId => FindClaimValue(NoelleClaimTypes.ClientId);

    /// <inheritdoc/>
    public string? Subject => FindClaimValue(NoelleClaimTypes.Subject) ?? FindClaimValue(ClaimTypes.NameIdentifier);

    /// <inheritdoc/>
    public string? UserId => FindClaimValue(NoelleClaimTypes.UserId) ?? FindClaimValue(ClaimTypes.NameIdentifier);

    /// <inheritdoc/>
    public string? OrganizationUnitId => FindClaimValue(NoelleClaimTypes.OrganizationUnitId);

    /// <inheritdoc/>
    public string? UserName => FindClaimValue(NoelleClaimTypes.UserName) ?? FindClaimValue(ClaimTypes.Name);

    /// <inheritdoc/>
    public string? GivenName => FindClaimValue(NoelleClaimTypes.GivenName) ?? FindClaimValue(ClaimTypes.GivenName);

    /// <inheritdoc/>
    public string? Surname => FindClaimValue(NoelleClaimTypes.Surname) ?? FindClaimValue(ClaimTypes.Surname);

    /// <inheritdoc/>
    public string? MiddleName => FindClaimValue(NoelleClaimTypes.MiddleName);

    /// <inheritdoc/>
    public string? NickName => FindClaimValue(NoelleClaimTypes.NickName);

    /// <inheritdoc/>
    public string? Email => FindClaimValue(NoelleClaimTypes.Email) ?? FindClaimValue(ClaimTypes.Email);

    /// <inheritdoc/>
    public bool EmailConfirmed => FindClaimBooleanValue(NoelleClaimTypes.EmailVerified);

    /// <inheritdoc/>
    public string? PhoneNumber => FindClaimValue(NoelleClaimTypes.PhoneNumber);

    /// <inheritdoc/>
    public bool PhoneNumberConfirmed => FindClaimBooleanValue(NoelleClaimTypes.PhoneNumberVerified);

    /// <inheritdoc/>
    public string? Gender => FindClaimValue(NoelleClaimTypes.Gender) ?? FindClaimValue(ClaimTypes.Gender);

    /// <inheritdoc/>
    public DateTime? DateOfBirth => FindClaimDateTimeValue(NoelleClaimTypes.DateOfBirth) ?? FindClaimDateTimeValue(ClaimTypes.DateOfBirth);

    /// <inheritdoc/>
    public string[] Roles => FindClaimValues(NoelleClaimTypes.Role)
        .Concat(FindClaimValues(ClaimTypes.Role))
        .Distinct()
        .ToArray();

    /// <inheritdoc/>
    public string[] Permissions => FindClaimValues(NoelleClaimTypes.Permission);

    /// <inheritdoc/>
    public Claim[] Claims => _principalProvider.Principal?.Claims.ToArray() ?? [];

    /// <inheritdoc/>
    public Claim? FindClaim(string claimType)
    {
        return _principalProvider.Principal?.FindFirst(claimType);
    }

    /// <inheritdoc/>
    public Claim[] FindClaims(string claimType)
    {
        return _principalProvider.Principal?.FindAll(claimType).ToArray() ?? [];
    }

    /// <inheritdoc/>
    public string? FindClaimValue(string claimType)
    {
        return _principalProvider.Principal?.FindFirst(claimType)?.Value;
    }

    /// <inheritdoc/>
    public string[] FindClaimValues(string claimType)
    {
        return _principalProvider.Principal?.FindAll(claimType).Select(c => c.Value).ToArray() ?? [];
    }

    /// <inheritdoc/>
    public bool HasPermission(string permission)
    {
        return Permissions.Any(p => p == permission);
    }

    /// <inheritdoc/>
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
