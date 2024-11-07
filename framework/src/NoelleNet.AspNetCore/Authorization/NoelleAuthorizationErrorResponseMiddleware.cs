using Microsoft.Extensions.Localization;
using NoelleNet.AspNetCore.ExceptionHandling.Localization;
using NoelleNet.ExceptionHandling;
using NoelleNet.Http;
using System.Net;

namespace NoelleNet.AspNetCore.Authorization;

/// <summary>
/// 处理授权错误响应的中间件。如果用户无权限访问某资源，返回授权错误响应。
/// </summary>
/// <param name="next">下一个中间件请求委托。</param>
/// <param name="localizer">本地化 <see cref="IStringLocalizer{T}"/> 的实例</param>
public class NoelleAuthorizationErrorResponseMiddleware(RequestDelegate next, IStringLocalizer<NoelleExceptionHandlingResource> localizer)
{
    private readonly RequestDelegate _next = next;
    private readonly IStringLocalizer<NoelleExceptionHandlingResource> _localizer = localizer;

    /// <summary>
    /// 处理请求
    /// </summary>
    /// <param name="context">当前 <see cref="HttpContext"/> 。</param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context)
    {
        await _next.Invoke(context);

        if (context.Response.HasStarted || context.Response.StatusCode != (int)HttpStatusCode.Forbidden)
            return;

        var error = new NoelleErrorDetailDto(_localizer["ForbiddenErrorMessage"]) { Code = NoelleErrorCodeConstants.Forbidden };
        var response = new NoelleErrorResponseDto(error);
        await context.Response.WriteAsJsonAsync(response);
    }
}
