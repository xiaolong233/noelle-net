namespace NoelleNet.AspNetCore.Middlewares;

/// <summary>
/// 错误处理的扩展方法集
/// </summary>
public static class NoelleMiddlewareExtensions
{
    /// <summary>
    /// 启用身份认证失败时的错误信息。必须放在 UseAuthentication() 之前
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseUnauthorizedError(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<NoelleUnauthorizedErrorMiddleware>();
    }

    /// <summary>
    /// 启用权限不足时的错误信息。必须放在 UseAuthorization() 之前
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseForbiddenError(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<NoelleForbiddenErrorMiddleware>();
    }
}
