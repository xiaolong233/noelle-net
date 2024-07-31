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
        public const string ValidationFailed = "ValidationFailed";

        /// <summary>
        /// 身份认证失败
        /// </summary>
        public const string Unauthorized = "Unauthorized";

        /// <summary>
        /// 权限不足
        /// </summary>
        public const string Forbidden = "Forbidden";

        /// <summary>
        /// 资源未找到
        /// </summary>
        public const string NotFound = "NotFound";

        /// <summary>
        /// 服务器错误
        /// </summary>
        public const string InternalServerError = "InternalServerError";
    }
}
