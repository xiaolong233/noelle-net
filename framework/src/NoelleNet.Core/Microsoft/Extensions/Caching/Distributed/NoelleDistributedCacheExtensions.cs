using System.Text.Json;

namespace Microsoft.Extensions.Caching.Distributed;

/// <summary>
/// 分布式缓存扩展方法集
/// </summary>
public static class NoelleDistributedCacheExtensions
{
    /// <summary>
    /// 获取与指定的键关联的值
    /// </summary>
    /// <typeparam name="TValue">要获取的值的类型</typeparam>
    /// <param name="cache"><see cref="IDistributedCache"/> 的实例</param>
    /// <param name="key">要获取的值的键</param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    public static async Task<TValue?> GetAsync<TValue>(this IDistributedCache cache, string key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        string? value = await cache.GetStringAsync(key, cancellationToken);
        if (string.IsNullOrWhiteSpace(value))
            return default;

        return JsonSerializer.Deserialize<TValue>(value);
    }

    /// <summary>
    /// 获取与指定的键关联的值
    /// </summary>
    /// <typeparam name="TValue">要获取的值的类型</typeparam>
    /// <param name="cache"><see cref="IDistributedCache"/> 的实例</param>
    /// <param name="key">要获取的值的键</param>
    /// <param name="factory">返回需要缓存的值，并配置缓存选项</param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task<TValue?> GetOrCreateAsync<TValue>(this IDistributedCache cache, string key, Func<DistributedCacheEntryOptions, Task<TValue>> factory, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(factory);

        string? value = await cache.GetStringAsync(key, cancellationToken);
        if (string.IsNullOrWhiteSpace(value))
        {
            var options = new DistributedCacheEntryOptions();
            var item = await factory.Invoke(options);
            await cache.SetAsync(key, item, options, cancellationToken);
            return item;
        }
        return JsonSerializer.Deserialize<TValue>(value);
    }

    /// <summary>
    /// 设置与指定的键关联的值
    /// </summary>
    /// <typeparam name="TValue">要设置的值的类型</typeparam>
    /// <param name="cache"><see cref="IDistributedCache"/> 的实例</param>
    /// <param name="key">要设置的值的键</param>
    /// <param name="value">要设置的值</param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    public static Task SetAsync<TValue>(this IDistributedCache cache, string key, TValue value, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return cache.SetStringAsync(key, JsonSerializer.Serialize(value), cancellationToken);
    }

    /// <summary>
    /// 设置与指定的键关联的值
    /// </summary>
    /// <typeparam name="TValue">要设置的值的类型</typeparam>
    /// <param name="cache"><see cref="IDistributedCache"/> 的实例</param>
    /// <param name="key">要设置的值的键</param>
    /// <param name="value">要设置的值</param>
    /// <param name="options">缓存选项</param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    public static Task SetAsync<TValue>(this IDistributedCache cache, string key, TValue value, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return cache.SetStringAsync(key, JsonSerializer.Serialize(value), options, cancellationToken);
    }

    /// <summary>
    /// 设置与指定的键关联的值
    /// </summary>
    /// <typeparam name="T">要设置的值的类型</typeparam>
    /// <param name="cache"><see cref="IDistributedCache"/> 的实例</param>
    /// <param name="key">要设置的值的键</param>
    /// <param name="value">要设置的值</param>
    /// <param name="configure">缓存选项的配置方式</param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    public static Task SetAsync<T>(this IDistributedCache cache, string key, T value, Action<DistributedCacheEntryOptions> configure, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var options = new DistributedCacheEntryOptions();
        configure?.Invoke(options);

        return cache.SetAsync(key, value, options, cancellationToken);
    }
}
