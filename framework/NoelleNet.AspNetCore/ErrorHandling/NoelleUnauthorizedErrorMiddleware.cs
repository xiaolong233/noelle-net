namespace NoelleNet.AspNetCore.ErrorHandling;

/// <summary>
/// 身份认证失败时，返回错误信息
/// </summary>
/// <param name="next"></param>
public class NoelleUnauthorizedErrorMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        await _next.Invoke(context);

        if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
            await context.Response.WriteAsJsonAsync(new NoelleErrorResponseDto(new NoelleErrorDetailDto("身份认证失败") { Code = NoelleConstants.ErrorCodes.Unauthorized }));
    }
}
