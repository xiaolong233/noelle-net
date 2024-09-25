namespace NoelleNet.AspNetCore;

/// <summary>
/// 常量表
/// </summary>
public static class NoelleConstants
{
    /// <summary>
    /// 错误代码
    /// </summary>
    public static class ErrorCodes
    {
        /// <summary>
        /// 模型验证失败
        /// </summary>
        public const string ValidationFailed = "validation_failed";

        /// <summary>
        /// 身份认证失败
        /// </summary>
        public const string Unauthorized = "unauthorized";

        /// <summary>
        /// 权限不足
        /// </summary>
        public const string Forbidden = "forbidden";

        /// <summary>
        /// 资源未找到
        /// </summary>
        public const string NotFound = "not_found";

        /// <summary>
        /// 请求与服务器的当前状态冲突
        /// </summary>
        public const string Conflict = "conflict";

        /// <summary>
        /// 服务器错误
        /// </summary>
        public const string InternalServerError = "internal_server_error";

        /// <summary>
        /// 远程服务调用失败
        /// </summary>
        public const string RemoteCallFailed = "remote_call_failed";
    }
}
