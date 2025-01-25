using NoelleNet.Security.Claims;
using System.Security.Claims;

namespace NoelleNet.AspNetCore.Security.Claims;

/// <summary>
/// 基于 <see cref="HttpContext"/> 的 <see cref="ICurrentPrincipalProvider"/> 实现
/// </summary>
/// <param name="contextAccessor">提供对当前 <see cref="HttpContext"/> 的访问</param>
public class NoelleHttpContextCurrentPrincipalProvider(IHttpContextAccessor contextAccessor) : ICurrentPrincipalProvider
{
    private readonly IHttpContextAccessor _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));

    public ClaimsPrincipal? Principal => _contextAccessor.HttpContext?.User;
}
