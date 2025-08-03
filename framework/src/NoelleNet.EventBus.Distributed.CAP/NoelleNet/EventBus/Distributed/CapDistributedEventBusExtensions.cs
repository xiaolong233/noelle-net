using DotNetCore.CAP;
using DotNetCore.CAP.Internal;
using Microsoft.Extensions.DependencyInjection;
using NoelleNet.EventBus.Abstractions.Distributed;
using NoelleNet.EventBus.Distributed.CAP;

namespace NoelleNet.EventBus.Distributed;

/// <summary>
/// CAP 分布式事件总线的扩展方法集
/// </summary>
public static class CapDistributedEventBusExtensions
{
    /// <summary>
    /// 使用 CAP
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="setupAction">配置 CAP</param>
    /// <returns></returns>
    public static DistributedEventBusConfiguration UseCap(this DistributedEventBusConfiguration configuration, Action<CapOptions> setupAction)
    {
        // 注册事件总线
        configuration.Services.AddScoped<IDistributedEventBus, CapDistributedEventBus>();

        // 注册 CAP
        configuration.Services.AddSingleton<IConsumerServiceSelector, NoelleConsumerServiceSelector>();
        configuration.Services.AddCap(setupAction);

        return configuration;
    }
}
