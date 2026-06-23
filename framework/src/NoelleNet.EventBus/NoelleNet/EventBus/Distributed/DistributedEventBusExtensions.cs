using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NoelleNet.EventBus.Abstractions.Distributed;

namespace NoelleNet.EventBus.Distributed;

/// <summary>
/// 分布式事件总线扩展方法集
/// </summary>
public static class DistributedEventBusExtensions
{
    /// <summary>
    /// 添加分布式事件总线
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static IServiceCollection AddDistributedEventBus(this IServiceCollection services, Action<DistributedEventBusConfiguration> configure)
    {
        var configuration = new DistributedEventBusConfiguration(services);
        configure?.Invoke(configuration);

        // 扫描所有 IDistributedEventHandler<> 实现
        var handlerInterfaceType = typeof(IDistributedEventHandler<>);
        var handlerEventTypePairs = configuration.AssembliesToRegister
            .SelectMany(x => x.GetTypes())
            .Where(x => !x.IsAbstract && !x.IsInterface)
            .SelectMany(x => x.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType)
                .Select(i => (x, i.GetGenericArguments()[0]))
            )
            .ToList();

        // 将扫描结果写入 Options（供 NoelleConsumerServiceSelector 通过 IOptions<T> 注入使用）
        services.AddOptions<NoelleDistributedEventBusOptions>()
            .Configure(options => options.HandlerEventTypePairs = handlerEventTypePairs);

        foreach (var (handlerType, eventType) in handlerEventTypePairs)
        {
            var interfaceType = handlerInterfaceType.MakeGenericType(eventType);
            services.TryAddTransient(interfaceType, handlerType);
        }

        return services;
    }
}
