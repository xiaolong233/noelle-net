namespace NoelleNet.ExceptionHandling;

/// <summary>
/// 错误代码表
/// </summary>
public static class NoelleErrorCodes
{
    /// <summary>
    /// 模型验证失败
    /// </summary>
    public static string ValidationFailed { get; set; } = "ValidationFailed";

    /// <summary>
    /// 身份认证失败
    /// </summary>
    public static string Unauthorized { get; set; } = "Unauthorized";

    /// <summary>
    /// 禁止操作
    /// </summary>
    public static string Forbidden { get; set; } = "Forbidden";

    /// <summary>
    /// 资源未找到
    /// </summary>
    public static string NotFound { get; set; } = "NotFound";

    /// <summary>
    /// 请求与服务器的当前状态冲突
    /// </summary>
    public static string Conflict { get; set; } = "Conflict";

    /// <summary>
    /// 服务器错误
    /// </summary>
    public static string InternalServerError { get; set; } = "InternalServerError";

    /// <summary>
    /// 远程服务调用失败
    /// </summary>
    public static string RemoteCallFailed { get; set; } = "RemoteCallFailed";
}
