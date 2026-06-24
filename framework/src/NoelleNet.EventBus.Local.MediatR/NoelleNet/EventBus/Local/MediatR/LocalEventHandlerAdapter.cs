using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NoelleNet.EventBus.Abstractions.Local;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace NoelleNet.EventBus.Local.MediatR;

/// <summary>
/// 本地事件处理程序适配器
/// </summary>
public class LocalEventHandlerAdapter : INotificationHandler<LocalEventAdapter>
{
    private static readonly ConcurrentDictionary<Type, Func<object, object, CancellationToken, Task>> _handleDelegates = new();
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
        var handleDelegate = _handleDelegates.GetOrAdd(handlerType, CreateHandleDelegate);

        var handlers = _serviceProvider.GetServices(handlerType);
        foreach (var handler in handlers)
        {
            if (handler == null)
                continue;
            await handleDelegate(handler, notification.SourceEvent, cancellationToken);
        }
    }

    private static Func<object, object, CancellationToken, Task> CreateHandleDelegate(Type handlerType)
    {
        var method = handlerType.GetMethod(nameof(ILocalEventHandler<object>.HandleAsync))
            ?? throw new InvalidOperationException($"无法获取 {handlerType} 的处理方法");

        var handlerParam = Expression.Parameter(typeof(object), "handler");
        var eventParam = Expression.Parameter(typeof(object), "eventData");
        var ctParam = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

        var castHandler = Expression.Convert(handlerParam, handlerType);
        var eventType = method.GetParameters()[0].ParameterType;
        var castEvent = Expression.Convert(eventParam, eventType);
        var call = Expression.Call(castHandler, method, castEvent, ctParam);

        return Expression.Lambda<Func<object, object, CancellationToken, Task>>(call, handlerParam, eventParam, ctParam).Compile();
    }
}
