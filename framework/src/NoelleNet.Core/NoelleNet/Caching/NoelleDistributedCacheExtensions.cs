using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace NoelleNet.Caching;

/// <summary>
/// 分布式缓存扩展方法集
/// </summary>
public static class NoelleDistributedCacheExtensions
{
    /// <summary>
    /// 获取与此键关联的值
    /// </summary>
    /// <typeparam name="T">缓存值的类型</typeparam>
    /// <param name="cache">此方法扩展的实例</param>
    /// <param name="key">要获取的值的键</param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    public static async Task<T?> GetAsync<T>(this IDistributedCache cache, string key, CancellationToken cancellationToken = default)
    {
        string? value = await cache.GetStringAsync(key, cancellationToken);
        if (string.IsNullOrWhiteSpace(value))
            return default;
        return JsonSerializer.Deserialize<T>(value);
    }

    /// <summary>
    /// 获取与此键关联的值（如果存在），则把工厂函数返回的值写入到缓存并返回
    /// </summary>
    /// <typeparam name="T">缓存值的类型</typeparam>
    /// <param name="cache">此方法扩展的实例</param>
    /// <param name="key">要获取的值的键</param>
    /// <param name="factory">如果缓存中不存在键，则通过工厂函数创建与键关联的值</param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task<T?> GetOrCreateAsync<T>(this IDistributedCache cache, string key, Func<DistributedCacheEntryOptions, Task<T>> factory, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(factory);
        string? value = await cache.GetStringAsync(key, cancellationToken);
        if (string.IsNullOrWhiteSpace(value))
        {
            DistributedCacheEntryOptions options = new();
            var item = await factory(options);
            await cache.SetAsync(key, item, options, cancellationToken);
            return item;
        }
        return JsonSerializer.Deserialize<T>(value);
    }

    /// <summary>
    /// 设置具有给定键的值
    /// </summary>
    /// <typeparam name="T">缓存值的类型</typeparam>
    /// <param name="cache">此方法扩展的实例</param>
    /// <param name="key">要设置的值的键</param>
    /// <param name="value">要在缓存中设置的值</param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    public static Task SetAsync<T>(this IDistributedCache cache, string key, T value, CancellationToken cancellationToken = default)
    {
        return cache.SetStringAsync(key, JsonSerializer.Serialize(value), cancellationToken);
    }

    /// <summary>
    /// 设置具有给定键的值
    /// </summary>
    /// <typeparam name="T">缓存值的类型</typeparam>
    /// <param name="cache">此方法扩展的实例</param>
    /// <param name="key">要设置的值的键</param>
    /// <param name="value">要在缓存中设置的值</param>
    /// <param name="options">值的缓存选项</param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    public static Task SetAsync<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default)
    {
        return cache.SetStringAsync(key, JsonSerializer.Serialize(value), options, cancellationToken);
    }

    /// <summary>
    /// 设置具有给定键的值
    /// </summary>
    /// <typeparam name="T">缓存值的类型</typeparam>
    /// <param name="cache">此方法扩展的实例</param>
    /// <param name="key">要设置的值的键</param>
    /// <param name="value">要在缓存中设置的值</param>
    /// <param name="configure">配置值的缓存选项</param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    public static Task SetAsync<T>(this IDistributedCache cache, string key, T value, Action<DistributedCacheEntryOptions> configure, CancellationToken cancellationToken = default)
    {
        DistributedCacheEntryOptions options = new();
        configure?.Invoke(options);
        return cache.SetAsync(key, value, options, cancellationToken);
    }
}
