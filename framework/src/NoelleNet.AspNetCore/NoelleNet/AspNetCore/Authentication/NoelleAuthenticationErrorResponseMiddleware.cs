using Microsoft.Extensions.Localization;
using NoelleNet.AspNetCore.ExceptionHandling.Localization;
using NoelleNet.ExceptionHandling;
using NoelleNet.Http;
using System.Net;

namespace NoelleNet.AspNetCore.Authentication;

/// <summary>
/// 处理身份认证错误响应的中间件。如果请求未通过身份认证，返回身份认证错误响应
/// </summary>
/// <param name="next">下一个处理 HTTP 请求的函数</param>
/// <param name="localizer">字符串本地化器</param>
public class NoelleAuthenticationErrorResponseMiddleware(RequestDelegate next, IStringLocalizer<NoelleExceptionHandlingResource> localizer)
{
    private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));
    private readonly IStringLocalizer<NoelleExceptionHandlingResource> _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));

    /// <summary>
    /// 处理请求。
    /// </summary>
    /// <param name="context">当前的 <see cref="HttpContext"/></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context)
    {
        await _next.Invoke(context);

        if (context.Response.HasStarted || context.Response.StatusCode != (int)HttpStatusCode.Unauthorized)
            return;

        var error = new NoelleErrorDto(_localizer["UnauthorizedErrorMessage"]) { Code = NoelleErrorCodeConstants.Unauthorized };
        var response = new NoelleErrorResponseDto(error);
        await context.Response.WriteAsJsonAsync(response);
    }
}
