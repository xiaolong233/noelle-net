﻿using Microsoft.Extensions.Localization;
using NoelleNet.AspNetCore.ExceptionHandling.Localization;
using NoelleNet.ExceptionHandling;
using NoelleNet.Http;
using System.Net;

namespace NoelleNet.AspNetCore.Authentication;

/// <summary>
/// 处理身份认证错误响应的中间件。如果请求未通过身份认证，返回身份认证错误响应。
/// </summary>
/// <param name="next">下一个中间件请求委托。</param>
/// <param name="localizer">本地化 <see cref="IStringLocalizer{T}"/> 的实例</param>
public class NoelleAuthenticationErrorResponseMiddleware(RequestDelegate next, IStringLocalizer<NoelleExceptionHandlingResource> localizer)
{
    private readonly RequestDelegate _next = next;
    private readonly IStringLocalizer<NoelleExceptionHandlingResource> _localizer = localizer;

    /// <summary>
    /// 处理请求。
    /// </summary>
    /// <param name="context">当前 <see cref="HttpContext"/> 。</param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context)
    {
        await _next.Invoke(context);

        if (context.Response.HasStarted || context.Response.StatusCode != (int)HttpStatusCode.Unauthorized)
            return;

        var error = new NoelleErrorDetailDto(_localizer["UnauthorizedErrorMessage"]) { Code = NoelleErrorCodeConstants.Unauthorized };
        var response = new NoelleErrorResponseDto(error);
        await context.Response.WriteAsJsonAsync(response);
    }
}