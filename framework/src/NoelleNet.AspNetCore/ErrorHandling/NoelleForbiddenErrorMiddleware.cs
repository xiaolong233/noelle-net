namespace NoelleNet.AspNetCore.ErrorHandling;

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
            await context.Response.WriteAsJsonAsync(new NoelleErrorResponseDto(new NoelleErrorDetailDto("权限不足") { Code = NoelleConstants.ErrorCodes.Forbidden }));
    }
}
