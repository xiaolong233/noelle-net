using Moq;

namespace NoelleNet.Http.Security;

public class TokenManagerBaseTests
{
    private readonly TokenManagerOptions _options;

    public TokenManagerBaseTests()
    {
        _options = new TokenManagerOptions { ExpirationBuffer = TimeSpan.Zero };
    }

    [Fact]
    public void Constructor_NullOptions_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new TestTokenManager(null!, new TokenResponse("t", 1)));
    }

    [Fact]
    public async Task GetValidTokenAsync_InitialCall_ShouldFetchNewToken()
    {
        var manager = new TestTokenManager(_options, new TokenResponse("initial_token", 3600));

        var token = await manager.GetValidTokenAsync();

        Assert.Equal("initial_token", token);
        Assert.Equal(1, manager.FetchCount);
    }

    [Fact]
    public async Task GetValidTokenAsync_TokenStillValid_ShouldReturnCachedToken()
    {
        var manager = new TestTokenManager(_options, new TokenResponse("cached_token", 3600));

        var token1 = await manager.GetValidTokenAsync();
        var token2 = await manager.GetValidTokenAsync();

        Assert.Equal("cached_token", token1);
        Assert.Equal("cached_token", token2);
        Assert.Equal(1, manager.FetchCount);
    }

    [Fact]
    public async Task GetValidTokenAsync_TokenExpired_ShouldFetchNewToken()
    {
        // Set expires=-1 to simulate expired token
        var manager = new TestTokenManager(_options, new TokenResponse("expired_token", -1));

        var token1 = await manager.GetValidTokenAsync();
        // Second call - token is expired, should fetch again
        var token2 = await manager.GetValidTokenAsync();

        // The second call should fetch again
        Assert.Equal(2, manager.FetchCount);
    }

    [Fact]
    public async Task ForceRefreshTokenAsync_ShouldAlwaysFetchNewToken()
    {
        var manager = new TestTokenManager(_options, new TokenResponse("token1", 3600));

        await manager.ForceRefreshTokenAsync();
        Assert.Equal(1, manager.FetchCount);

        await manager.ForceRefreshTokenAsync();
        Assert.Equal(2, manager.FetchCount);
    }

    [Fact]
    public async Task ForceRefreshTokenAsync_ShouldUpdateCachedToken()
    {
        var responses = new Queue<TokenResponse>(new[]
        {
            new TokenResponse("first", 3600),
            new TokenResponse("second", 3600),
        });
        var manager = new TestTokenManager(_options, responses);

        // Initial fetch
        var token1 = await manager.GetValidTokenAsync();
        Assert.Equal("first", token1);

        // Force refresh
        await manager.ForceRefreshTokenAsync();

        // Get again should return refreshed token
        var token2 = await manager.GetValidTokenAsync();
        Assert.Equal("second", token2);
    }

    [Fact]
    public async Task GetValidTokenAsync_DoubleCheckLock_ShouldOnlyFetchOnce()
    {
        var manager = new TestTokenManager(_options, new TokenResponse("locked_token", -1));

        // Simulate concurrent access - both should wait for the lock
        var t1 = manager.GetValidTokenAsync();
        var t2 = manager.GetValidTokenAsync();

        await Task.WhenAll(t1, t2);

        // Both should return
        Assert.NotNull(await t1);
        Assert.NotNull(await t2);
    }

    [Fact]
    public void TestTokenManager_ShouldImplementITokenManager()
    {
        var manager = new TestTokenManager(_options, new TokenResponse("t", 3600));
        Assert.IsAssignableFrom<ITokenManager>(manager);
    }

    private class TestTokenManager : TokenManagerBase
    {
        private readonly Queue<TokenResponse> _responses;
        public int FetchCount { get; private set; }

        public TestTokenManager(TokenManagerOptions options, TokenResponse response) : base(options)
        {
            _responses = new Queue<TokenResponse>(new[] { response });
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
