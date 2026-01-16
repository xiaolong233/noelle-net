using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace NoelleNet.EventBus.Local;

/// <summary>
/// 本地事件总线配置
/// </summary>
public class LocalEventBusConfiguration
{
    /// <summary>
    /// 创建一个新的 <see cref="LocalEventBusConfiguration"/> 实例
    /// </summary>
    /// <param name="services"></param>
    public LocalEventBusConfiguration(IServiceCollection services)
    {
        Services = services;
    }

    /// <summary>
    /// 服务集合
    /// </summary>
    public IServiceCollection Services { get; }

    private List<Assembly> _assembliesToRegister = [];
    /// <summary>
    /// 待注册的程序集
    /// </summary>
    public IReadOnlyCollection<Assembly> AssembliesToRegister => _assembliesToRegister.AsReadOnly();

    /// <summary>
    /// 从程序集注册服务
    /// </summary>
    /// <param name="assembly">目标程序集</param>
    /// <returns></returns>
    public LocalEventBusConfiguration RegisterServicesFromAssembly(Assembly assembly)
    {
        _assembliesToRegister.Add(assembly);
        _assembliesToRegister = [.. _assembliesToRegister.Distinct()];
        return this;
    }

    /// <summary>
    /// 从程序集注册服务
    /// </summary>
    /// <param name="assemblies">目标程序集</param>
    /// <returns></returns>
    public LocalEventBusConfiguration RegisterServicesFromAssemblies(params Assembly[] assemblies)
    {
        _assembliesToRegister.AddRange(assemblies);
        _assembliesToRegister = [.. _assembliesToRegister.Distinct()];
        return this;
    }
}
