using Microsoft.Extensions.Caching.Distributed;
using Moq;
using System.Text;
using System.Text.Json;

namespace Microsoft.Extensions.Caching.Distributed;

public class NoelleDistributedCacheExtensionsTests
{
    private readonly Mock<IDistributedCache> _cacheMock;

    public NoelleDistributedCacheExtensionsTests()
    {
        _cacheMock = new Mock<IDistributedCache>();
    }

    [Fact]
    public async Task GetAsync_NullCache_ShouldThrow()
    {
        IDistributedCache? cache = null;
        await Assert.ThrowsAsync<ArgumentNullException>(() => cache!.GetAsync<string>("key"));
    }

    [Fact]
    public async Task GetAsync_NullKey_ShouldThrow()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _cacheMock.Object.GetAsync<string>(null!));
    }

    [Fact]
    public async Task GetAsync_EmptyKey_ShouldThrow()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _cacheMock.Object.GetAsync<string>(""));
    }

    [Fact]
    public async Task GetAsync_CacheHit_ShouldDeserialize()
    {
        var obj = new TestDto { Name = "test", Value = 42 };
        var json = JsonSerializer.Serialize(obj);

        _cacheMock.Setup(c => c.GetAsync("key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes(json));

        var result = await _cacheMock.Object.GetAsync<TestDto>("key");

        Assert.NotNull(result);
        Assert.Equal("test", result!.Name);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public async Task GetAsync_CacheMiss_ShouldReturnDefault()
    {
        _cacheMock.Setup(c => c.GetAsync("key", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        var result = await _cacheMock.Object.GetAsync<TestDto>("key");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrCreateAsync_NullCache_ShouldThrow()
    {
        IDistributedCache? cache = null;
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            cache!.GetOrCreateAsync("key", _ => Task.FromResult("value")));
    }

    [Fact]
    public async Task GetOrCreateAsync_NullKey_ShouldThrow()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _cacheMock.Object.GetOrCreateAsync(null!, _ => Task.FromResult("value")));
    }

    [Fact]
    public async Task GetOrCreateAsync_NullFactory_ShouldThrow()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _cacheMock.Object.GetOrCreateAsync<string>("key", null!));
    }

    [Fact]
    public async Task GetOrCreateAsync_CacheHit_ShouldReturnCached()
    {
        var json = JsonSerializer.Serialize("cached_value");

        _cacheMock.Setup(c => c.GetAsync("key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes(json));

        var factoryCalled = false;
        var result = await _cacheMock.Object.GetOrCreateAsync("key", _ =>
        {
            factoryCalled = true;
            return Task.FromResult("new_value");
        });

        Assert.False(factoryCalled);
        Assert.Equal("cached_value", result);
    }

    [Fact]
    public async Task GetOrCreateAsync_CacheMiss_ShouldCallFactory()
    {
        _cacheMock.Setup(c => c.GetAsync("key", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        var result = await _cacheMock.Object.GetOrCreateAsync("key", _ => Task.FromResult("new_value"));

        Assert.Equal("new_value", result);
        _cacheMock.Verify(c => c.SetAsync(
            "key",
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SetAsync_NullCache_ShouldThrow()
    {
        IDistributedCache? cache = null;
        await Assert.ThrowsAsync<ArgumentNullException>(() => cache!.SetAsync("key", "value"));
    }

    [Fact]
    public async Task SetAsync_NullKey_ShouldThrow()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _cacheMock.Object.SetAsync(null!, "value"));
    }

    [Fact]
    public async Task SetAsync_ShouldSerializeValue()
    {
        // SetAsync<T> (2-param) calls SetStringAsync extension -> SetAsync on interface
        _cacheMock.Setup(c => c.SetAsync("key", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _cacheMock.Object.SetAsync("key", "test_value");

        _cacheMock.Verify(c => c.SetAsync(
            "key",
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SetAsync_WithOptions_ShouldCallSetAsync()
    {
        _cacheMock.Setup(c => c.SetAsync("key", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var options = new DistributedCacheEntryOptions();
        await _cacheMock.Object.SetAsync("key", "value", options);

        _cacheMock.Verify(c => c.SetAsync(
            "key",
            It.IsAny<byte[]>(),
            options,
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SetAsync_WithConfigure_ShouldCallSetAsync()
    {
        _cacheMock.Setup(c => c.SetAsync("key", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _cacheMock.Object.SetAsync("key", "value", o => o.AbsoluteExpiration = DateTimeOffset.Now.AddHours(1));

        _cacheMock.Verify(c => c.SetAsync(
            "key",
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    public class TestDto
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
