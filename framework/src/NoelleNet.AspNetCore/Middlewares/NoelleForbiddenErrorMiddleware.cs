using Microsoft.Extensions.Localization;
using NoelleNet.AspNetCore.ExceptionHandling.Models;
using NoelleNet.AspNetCore.Localization;

namespace NoelleNet.AspNetCore.Middlewares;

/// <summary>
/// 权限不足时，返回错误信息
/// </summary>
/// <param name="next"></param>
public class NoelleForbiddenErrorMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        await _next.Invoke(context);

        if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
        {
            var localizer = context.RequestServices.GetRequiredService<IStringLocalizer<NoelleAspNetCoreResource>>();
            await context.Response.WriteAsJsonAsync(new NoelleErrorResponseDto(new NoelleErrorDetailDto(localizer[NoelleConstants.ErrorCodes.Forbidden]) { Code = NoelleConstants.ErrorCodes.Forbidden }));
        }
    }
}
