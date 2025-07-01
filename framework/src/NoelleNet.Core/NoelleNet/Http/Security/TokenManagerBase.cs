namespace NoelleNet.Http.Security;

/// <summary>
/// 访问令牌管理器基类
/// </summary>
public abstract class TokenManagerBase : ITokenManager
{
    private string? _accessToken;
    private DateTime _expiryTime;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly TokenManagerOptions _options;

    /// <summary>
    /// 创建一个新的 <see cref="TokenManagerBase"/> 实例
    /// </summary>
    /// <param name="options">访问令牌管理器选项</param>
    /// <exception cref="ArgumentNullException"></exception>
    protected TokenManagerBase(TokenManagerOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// 强制刷新访问令牌
    /// </summary>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    public async Task ForceRefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var tokenResponse = await FetchNewTokenAsync(cancellationToken);
            _accessToken = tokenResponse.AccessToken;
            _expiryTime = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - _options.ExpirationBuffer.TotalSeconds);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// 获取有效的访问令牌
    /// </summary>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    public async Task<string> GetValidTokenAsync(CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _expiryTime)
        {
            return _accessToken;
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            // 双重检查锁定
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _expiryTime)
            {
                return _accessToken;
            }

            var tokenResponse = await FetchNewTokenAsync(cancellationToken);
            _accessToken = tokenResponse.AccessToken;
            _expiryTime = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - _options.ExpirationBuffer.TotalSeconds);
            return _accessToken;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// 获取新令牌
    /// </summary>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    protected abstract Task<TokenResponse> FetchNewTokenAsync(CancellationToken cancellationToken = default);
}
