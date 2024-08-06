using System.Security.Claims;

namespace NoelleNet.Security.Claims;

public class NoelleEmptyCurrentPrincipalProvider : ICurrentPrincipalProvider
{
    public ClaimsPrincipal? Principal => new ClaimsPrincipal();
}
