namespace NoelleNet.AspNetCore.Authorization;

/// <summary>
/// 提供与授权相关的扩展方法，用于在应用程序中配置中间件。
/// </summary>
public static class NoelleAuthorizationExtensions
{
    /// <summary>
    /// 向应用程序管道中添加处理授权错误响应的中间件。
    /// </summary>
    /// <param name="app">应用程序构建器 <see cref="IApplicationBuilder"/> 的实例</param>
    /// <returns></returns>
    public static IApplicationBuilder UseAuthorizationErrorResponse(this IApplicationBuilder app)
    {
        app.UseMiddleware<NoelleAuthorizationErrorResponseMiddleware>();
        return app;
    }
}
