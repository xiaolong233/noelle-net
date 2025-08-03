using Microsoft.Extensions.DependencyInjection;
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

        // 注册所有分布式事件处理器
        var handlerInterfaceType = typeof(IDistributedEventHandler<>);

        var handlerMateInfos = configuration.AssembliesToRegister
            .SelectMany(x => x.GetTypes())
            .Where(x => !x.IsAbstract && !x.IsInterface)
            .SelectMany(x => x.GetInterfaces()
                .Where(s => s.IsGenericType && s.GetGenericTypeDefinition() == handlerInterfaceType)
                .Select(s => new { HandlerType = x, EventType = s.GetGenericArguments()[0] })
             );

        foreach (var handlerMateInfo in handlerMateInfos)
        {
            var interfaceType = handlerInterfaceType.MakeGenericType(handlerMateInfo.EventType);

            services.AddTransient(interfaceType, handlerMateInfo.HandlerType);
        }

        return services;
    }
}
