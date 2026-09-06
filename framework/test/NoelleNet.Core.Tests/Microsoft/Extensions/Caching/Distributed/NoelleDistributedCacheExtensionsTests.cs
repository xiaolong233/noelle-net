using Microsoft.Extensions.Caching.Distributed;
using Moq;
using System.Text;
using System.Text.Json;

namespace Microsoft.Extensions.Caching.Distributed;

/// <summary>
/// <see cref="NoelleDistributedCacheExtensions"/> 的单元测试：泛型缓存读写的序列化行为
/// </summary>
public class NoelleDistributedCacheExtensionsTests
{
    private readonly Mock<IDistributedCache> _cacheMock = new();

    /// <summary>
    /// 命中缓存时应反序列化为目标类型
    /// </summary>
    [Fact]
    public async Task GetAsync_CacheHit_ShouldDeserialize()
    {
        var json = JsonSerializer.Serialize(new TestDto { Name = "test", Value = 42 });
        _cacheMock.Setup(c => c.GetAsync("key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes(json));

        var result = await _cacheMock.Object.GetAsync<TestDto>("key");

        Assert.NotNull(result);
        Assert.Equal("test", result!.Name);
        Assert.Equal(42, result.Value);
    }

    /// <summary>
    /// 未命中缓存时应返回默认值
    /// </summary>
    [Fact]
    public async Task GetAsync_CacheMiss_ShouldReturnDefault()
    {
        _cacheMock.Setup(c => c.GetAsync("key", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        Assert.Null(await _cacheMock.Object.GetAsync<TestDto>("key"));
    }

    /// <summary>
    /// GetOrCreateAsync 命中缓存时不应调用工厂
    /// </summary>
    [Fact]
    public async Task GetOrCreateAsync_CacheHit_ShouldReturnCachedWithoutFactory()
    {
        _cacheMock.Setup(c => c.GetAsync("key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize("cached_value")));
        var factoryCalled = false;

        var result = await _cacheMock.Object.GetOrCreateAsync("key", _ =>
        {
            factoryCalled = true;
            return Task.FromResult("new_value");
        });

        Assert.False(factoryCalled);
        Assert.Equal("cached_value", result);
    }

    /// <summary>
    /// GetOrCreateAsync 未命中时应调用工厂并写入缓存
    /// </summary>
    [Fact]
    public async Task GetOrCreateAsync_CacheMiss_ShouldCallFactoryAndSetCache()
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

    /// <summary>
    /// SetAsync 三种重载（无选项/显式选项/配置委托）都应序列化并写入缓存
    /// </summary>
    [Fact]
    public async Task SetAsync_AllOverloads_ShouldSerializeAndWrite()
    {
        await _cacheMock.Object.SetAsync("key1", "value");
        await _cacheMock.Object.SetAsync("key2", "value", new DistributedCacheEntryOptions());
        await _cacheMock.Object.SetAsync("key3", "value", o => o.AbsoluteExpiration = DateTimeOffset.Now.AddHours(1));

        _cacheMock.Verify(c => c.SetAsync(
                "key1", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.SetAsync(
                "key2", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.SetAsync(
                "key3", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// 关键参数为 null/空白时应抛出异常（cache 为 null、key 为 null/空）
    /// </summary>
    [Fact]
    public async Task InvalidArguments_ShouldThrow()
    {
        IDistributedCache? nullCache = null;

        await Assert.ThrowsAsync<ArgumentNullException>(() => nullCache!.GetAsync<string>("key"));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _cacheMock.Object.GetAsync<string>(null!));
        await Assert.ThrowsAsync<ArgumentException>(() => _cacheMock.Object.GetAsync<string>(""));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _cacheMock.Object.GetOrCreateAsync<string>("key", null!));
    }

    public class TestDto
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
