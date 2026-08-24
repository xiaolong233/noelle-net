namespace NoelleNet.AspNetCore.ExceptionHandling;

/// <summary>
/// ProblemDetails 的 Type 常量
/// </summary>
public static class NoelleProblemDetailsTypes
{
    /// <summary>
    /// 默认类型标识（未匹配到具体状态码时使用），默认值：about:blank
    /// </summary>
    public static string Default { get; set; } = "about:blank";

    /// <summary>
    /// 400 Bad Request，默认值：<see href="https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1"/>
    /// </summary>
    public static string BadRequest { get; set; } = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1";

    /// <summary>
    /// 401 Unauthorized，默认值：<see href="https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.2"/>
    /// </summary>
    public static string Unauthorized { get; set; } = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.2";

    /// <summary>
    /// 403 Forbidden，默认值：<see href="https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.4"/>
    /// </summary>
    public static string Forbidden { get; set; } = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.4";

    /// <summary>
    /// 404 Not Found，默认值：<see href="https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.5"/>
    /// </summary>
    public static string NotFound { get; set; } = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.5";

    /// <summary>
    /// 409 Conflict，默认值：<see href="https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.10"/>
    /// </summary>
    public static string Conflict { get; set; } = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.10";

    /// <summary>
    /// 500 Internal Server Error，默认值：<see href="https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.1"/>
    /// </summary>
    public static string InternalServerError { get; set; } = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.1";

    /// <summary>
    /// 501 Not Implemented，默认值：<see href="https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.2"/>
    /// </summary>
    public static string NotImplemented { get; set; } = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.2";
}
