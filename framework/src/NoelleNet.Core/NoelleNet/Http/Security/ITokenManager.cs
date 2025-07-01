namespace NoelleNet.Http.Security;

/// <summary>
/// 访问令牌管理器接口，用于管理对接平台的访问令牌生命周期
/// </summary>
public interface ITokenManager
{
    /// <summary>
    /// 获取有效的访问令牌
    /// </summary>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    Task<string> GetValidTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 强制刷新访问令牌
    /// </summary>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    Task ForceRefreshTokenAsync(CancellationToken cancellationToken = default);
}
