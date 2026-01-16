using NoelleNet.Security.Claims;
using System.Security.Claims;

namespace NoelleNet.AspNetCore.Security.Claims;

/// <summary>
/// 基于 <see cref="HttpContext"/> 的 <see cref="ICurrentPrincipalProvider"/> 实现
/// </summary>
public class NoelleHttpContextCurrentPrincipalProvider : ICurrentPrincipalProvider
{
    private readonly IHttpContextAccessor _contextAccessor;

    /// <summary>
    /// 创建一个新的 <see cref="NoelleHttpContextCurrentPrincipalProvider"/> 实例
    /// </summary>
    /// <param name="contextAccessor"><see cref="IHttpContextAccessor"/> 实例</param>
    /// <exception cref="ArgumentNullException"></exception>
    public NoelleHttpContextCurrentPrincipalProvider(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
    }

    /// <inheritdoc/>
    public ClaimsPrincipal? Principal => _contextAccessor.HttpContext?.User;
}
