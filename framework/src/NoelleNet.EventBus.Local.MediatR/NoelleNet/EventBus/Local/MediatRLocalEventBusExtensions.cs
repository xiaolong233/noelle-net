using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NoelleNet.EventBus.Abstractions.Local;
using NoelleNet.EventBus.Local.MediatR;

namespace NoelleNet.EventBus.Local;

/// <summary>
/// MediatR 本地事件总线扩展方法集
/// </summary>
public static class MediatRLocalEventBusExtensions
{
    /// <summary>
    /// 使用 MediatR
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="configure">配置 MediatR</param>
    /// <returns></returns>
    public static LocalEventBusConfiguration UseMediatR(this LocalEventBusConfiguration configuration, Action<MediatRServiceConfiguration> configure)
    {
        // 注册事件总线
        configuration.Services.AddScoped<ILocalEventBus, MediatRLocalEventBus>();

        // 注册MediatR
        configuration.Services.AddMediatR(configure);
        configuration.Services.TryAddTransient<INotificationHandler<LocalEventAdapter>, LocalEventHandlerAdapter>();

        return configuration;
    }
}
