using Microsoft.Extensions.DependencyInjection;
using NoelleNet.EventBus.Abstractions.Local;

namespace NoelleNet.EventBus.Local;

/// <summary>
/// 本地事件总线扩展方法集
/// </summary>
public static class LocalEventBusExtensions
{
    /// <summary>
    /// 添加本地事件总线
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddLocalEventBus(this IServiceCollection services, Action<LocalEventBusConfiguration> configuration)
    {
        var options = new LocalEventBusConfiguration(services);
        configuration?.Invoke(options);

        // 扫描所有程序集中的事件处理器
        var handlerInterfaceType = typeof(ILocalEventHandler<>);

        var handlerMateInfos = options.AssembliesToRegister
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
