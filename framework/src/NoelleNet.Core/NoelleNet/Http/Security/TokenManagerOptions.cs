namespace NoelleNet.Http.Security;

/// <summary>
/// 访问令牌管理器选项
/// </summary>
public class TokenManagerOptions
{
    /// <summary>
    /// 过期缓冲区
    /// </summary>
    public TimeSpan ExpirationBuffer { get; set; } = TimeSpan.FromMinutes(5);
}
