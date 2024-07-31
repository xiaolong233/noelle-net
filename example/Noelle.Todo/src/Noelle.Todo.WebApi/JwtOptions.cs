namespace Noelle.Todo.WebApi;

/// <summary>
/// Jwt的配置选项
/// </summary>
public class JwtOptions
{
    /// <summary>
    /// 配置节点名称
    /// </summary>
    public const string Jwt = "Jwt";

    /// <summary>
    /// 密钥
    /// </summary>
    public string SecurityKey { get; set; } = string.Empty;

    /// <summary>
    /// 多少秒后过期
    /// </summary>
    public int ExpireSeconds { get; set; }
}
