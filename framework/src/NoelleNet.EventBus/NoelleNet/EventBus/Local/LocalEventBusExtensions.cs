using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var options = new LocalEventBusConfiguration(services);
        configuration.Invoke(options);

        // 扫描所有程序集中的事件处理器
        var handlerInterfaceType = typeof(ILocalEventHandler<>);

        var handlerMetaInfos = options.AssembliesToRegister
            .SelectMany(AssemblyTypeLoader.GetLoadableTypes)
            .Where(x => !x.IsAbstract && !x.IsInterface)
            .SelectMany(x => x.GetInterfaces()
                .Where(s => s.IsGenericType && s.GetGenericTypeDefinition() == handlerInterfaceType)
                .Select(s => new { HandlerType = x, EventType = s.GetGenericArguments()[0] })
             );

        foreach (var handlerMetaInfo in handlerMetaInfos)
        {
            var interfaceType = handlerInterfaceType.MakeGenericType(handlerMetaInfo.EventType);

            services.TryAddEnumerable(ServiceDescriptor.Transient(interfaceType, handlerMetaInfo.HandlerType));
        }

        return services;
    }
}
