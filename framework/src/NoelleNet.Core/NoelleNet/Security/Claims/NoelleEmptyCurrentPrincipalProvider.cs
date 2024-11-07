using System.Security.Claims;

namespace NoelleNet.Security.Claims;

/// <summary>
/// <see cref="ICurrentPrincipalProvider"/> 的空实现。
/// </summary>
public class NoelleEmptyCurrentPrincipalProvider : ICurrentPrincipalProvider
{
    public ClaimsPrincipal? Principal => new();
}
