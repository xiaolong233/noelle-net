using Microsoft.Extensions.Localization;
using NoelleNet.AspNetCore.ExceptionHandling.Localization;
using NoelleNet.ExceptionHandling;
using NoelleNet.Http;
using System.Net;

namespace NoelleNet.AspNetCore.Authorization;

/// <summary>
/// 处理授权错误响应的中间件。如果用户无权限访问某资源，返回授权错误响应
/// </summary>
/// <param name="next">下一个处理 HTTP 请求的函数</param>
/// <param name="localizer">字符串本地化器</param>
public class NoelleAuthorizationErrorResponseMiddleware(RequestDelegate next, IStringLocalizer<NoelleExceptionHandlingResource> localizer)
{
    private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));
    private readonly IStringLocalizer<NoelleExceptionHandlingResource> _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));

    /// <summary>
    /// 处理请求
    /// </summary>
    /// <param name="context">当前的 <see cref="HttpContext"/></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context)
    {
        await _next.Invoke(context);

        if (context.Response.HasStarted || context.Response.StatusCode != (int)HttpStatusCode.Forbidden)
            return;

        var error = new NoelleErrorDto(NoelleErrorCodes.Forbidden, _localizer["ForbiddenErrorMessage"]);
        var response = new NoelleErrorResponseDto(error);
        await context.Response.WriteAsJsonAsync(response);
    }
}
