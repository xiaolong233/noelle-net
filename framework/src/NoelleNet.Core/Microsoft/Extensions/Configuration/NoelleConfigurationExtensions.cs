namespace Microsoft.Extensions.Configuration;

/// <summary>
/// <see cref="IConfiguration"/> 的扩展方法集
/// </summary>
public static class NoelleConfigurationExtensions
{
    /// <summary>
    /// 获取指定配置节点的值，如果键不存在则抛出异常
    /// </summary>
    /// <param name="configuration"><see cref="IConfiguration"/> 的实例</param>
    /// <param name="key">配置节点的键</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static string GetRequiredValue(this IConfiguration configuration, string key)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        string? value = configuration[key];
        return value ?? throw new InvalidOperationException($"配置项[{GetSectionFullPath(configuration, key)}]的值缺失");
    }

    /// <summary>
    /// 获取指定名称的连接字符串的配置值，如果不存在则抛出异常
    /// </summary>
    /// <param name="configuration"><see cref="IConfiguration"/> 的实例</param>
    /// <param name="name">连接字符串的名称</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static string GetRequiredConnectionString(this IConfiguration configuration, string name)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        string? value = configuration.GetConnectionString(name);
        return value ?? throw new InvalidOperationException($"配置项[{GetSectionFullPath(configuration, $"ConnectionStrings:{name}")}]的值缺失");
    }

    /// <summary>
    /// 返回配置节点的全路径
    /// </summary>
    /// <param name="configuration"><see cref="IConfiguration"/> 的实例</param>
    /// <param name="key">配置键</param>
    /// <returns></returns>
    private static string GetSectionFullPath(IConfiguration configuration, string key)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (configuration is not IConfigurationSection section)
            return key;
        return $"{section.Path}:{key}";
    }
}
