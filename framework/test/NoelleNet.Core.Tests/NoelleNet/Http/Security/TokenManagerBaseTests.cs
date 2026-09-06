namespace NoelleNet.Http.Security;

/// <summary>
/// <see cref="TokenManagerBase"/> 的单元测试：令牌缓存、过期刷新与并发双重检查锁
/// </summary>
public class TokenManagerBaseTests
{
    private readonly TokenManagerOptions _options = new() { ExpirationBuffer = TimeSpan.Zero };

    /// <summary>
    /// 首次调用应获取新令牌
    /// </summary>
    [Fact]
    public async Task GetValidTokenAsync_InitialCall_ShouldFetchNewToken()
    {
        var manager = new TestTokenManager(_options, new TokenResponse("initial_token", 3600));

        var token = await manager.GetValidTokenAsync();

        Assert.Equal("initial_token", token);
        Assert.Equal(1, manager.FetchCount);
    }

    /// <summary>
    /// 令牌未过期时应直接返回缓存，不重复获取
    /// </summary>
    [Fact]
    public async Task GetValidTokenAsync_TokenStillValid_ShouldReturnCachedToken()
    {
        var manager = new TestTokenManager(_options, new TokenResponse("cached_token", 3600));

        await manager.GetValidTokenAsync();
        var token = await manager.GetValidTokenAsync();

        Assert.Equal("cached_token", token);
        Assert.Equal(1, manager.FetchCount);
    }

    /// <summary>
    /// 令牌过期后应重新获取
    /// </summary>
    [Fact]
    public async Task GetValidTokenAsync_TokenExpired_ShouldFetchNewToken()
    {
        var manager = new TestTokenManager(_options, new TokenResponse("expired_token", -1));

        await manager.GetValidTokenAsync();
        await manager.GetValidTokenAsync();

        Assert.Equal(2, manager.FetchCount);
    }

    /// <summary>
    /// 强制刷新应无视缓存重新获取，并更新后续读取的令牌
    /// </summary>
    [Fact]
    public async Task ForceRefreshTokenAsync_ShouldFetchAndUpdateCache()
    {
        var manager = new TestTokenManager(_options, new Queue<TokenResponse>(
            [new("first", 3600), new("second", 3600)]));

        Assert.Equal("first", await manager.GetValidTokenAsync());

        await manager.ForceRefreshTokenAsync();

        Assert.Equal("second", await manager.GetValidTokenAsync());
        Assert.Equal(2, manager.FetchCount);
    }

    /// <summary>
    /// 并发获取时，双重检查锁应保证只获取一次令牌
    /// </summary>
    [Fact]
    public async Task GetValidTokenAsync_ConcurrentAccess_ShouldFetchOnlyOnce()
    {
        var manager = new TestTokenManager(_options, new TokenResponse("locked_token", 3600));

        await Task.WhenAll(manager.GetValidTokenAsync(), manager.GetValidTokenAsync());

        Assert.Equal(1, manager.FetchCount);
    }

    private class TestTokenManager : TokenManagerBase
    {
        private readonly Queue<TokenResponse> _responses;
        public int FetchCount { get; private set; }

        public TestTokenManager(TokenManagerOptions options, TokenResponse response) : base(options)
        {
            _responses = new Queue<TokenResponse>([response]);
        }

        public TestTokenManager(TokenManagerOptions options, Queue<TokenResponse> responses) : base(options)
        {
            _responses = responses;
        }

        protected override async Task<TokenResponse> FetchNewTokenAsync(CancellationToken cancellationToken = default)
        {
            FetchCount++;
            await Task.Delay(10, cancellationToken);
            return _responses.Count == 1 ? _responses.Peek() : _responses.Dequeue();
        }
    }
}
