namespace NoelleNet.AspNetCore;

/// <summary>
/// 常量表
/// </summary>
public static class ConstantTable
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
        /// 资源未找到
        /// </summary>
        public const string NotFound = "NotFound";

        /// <summary>
        /// 服务器错误
        /// </summary>
        public const string ServerError = "InternalServerError";
    }
}
