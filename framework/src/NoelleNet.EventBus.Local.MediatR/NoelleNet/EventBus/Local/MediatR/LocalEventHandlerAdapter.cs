using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NoelleNet.EventBus.Abstractions.Local;
using System.Collections.Concurrent;
using System.Reflection;

namespace NoelleNet.EventBus.Local.MediatR;

/// <summary>
/// 本地事件处理程序适配器
/// </summary>
public class LocalEventHandlerAdapter : INotificationHandler<LocalEventAdapter>
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> _handleMethods = new();
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 创建一个新的 <see cref="LocalEventHandlerAdapter"/> 实例
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public LocalEventHandlerAdapter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// 开始处理事件
    /// </summary>
    /// <param name="notification">待处理的事件</param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task Handle(LocalEventAdapter notification, CancellationToken cancellationToken)
    {
        var handlerType = typeof(ILocalEventHandler<>).MakeGenericType(notification.EventType);
        var handleMethod = _handleMethods.GetOrAdd(handlerType, t => t.GetMethod(nameof(ILocalEventHandler<object>.HandleAsync)) ?? throw new InvalidOperationException($"Could not get handle method for {handlerType}"));

        var handlers = _serviceProvider.GetServices(handlerType);
        foreach (var handler in handlers)
        {
            var task = (Task?)handleMethod.Invoke(handler, [notification.SourceEvent, cancellationToken]);
            if (task == null)
                continue;
            await task;
        }
    }
}
