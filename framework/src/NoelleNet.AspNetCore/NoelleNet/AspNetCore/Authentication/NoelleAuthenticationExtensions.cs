namespace NoelleNet.AspNetCore.Authentication;

/// <summary>
/// 提供身份认证相关的扩展方法，用于在应用程序中配置中间件
/// </summary>
public static class NoelleAuthenticationExtensions
{
    /// <summary>
    /// 向应用程序管道中添加设置身份认证错误的响应内容的中间件
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    /// <returns></returns>
    public static IApplicationBuilder UseAuthenticationErrorResponse(this IApplicationBuilder app)
    {
        app.UseMiddleware<NoelleAuthenticationErrorResponseMiddleware>();
        return app;
    }
}
