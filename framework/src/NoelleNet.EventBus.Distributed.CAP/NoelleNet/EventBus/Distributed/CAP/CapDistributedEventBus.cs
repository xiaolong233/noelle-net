using DotNetCore.CAP;
using NoelleNet.EventBus.Abstractions;
using NoelleNet.EventBus.Abstractions.Distributed;
using System.Reflection;

namespace NoelleNet.EventBus.Distributed.CAP;

/// <summary>
/// 基于 CAP 实现的分布式事件总线
/// </summary>
public class CapDistributedEventBus : IDistributedEventBus
{
    private readonly ICapPublisher _publisher;

    /// <summary>
    /// 创建一个新的 <see cref="CapDistributedEventBus"/> 实例
    /// </summary>
    /// <param name="publisher"><see cref="ICapPublisher"/> 实例</param>
    public CapDistributedEventBus(ICapPublisher publisher)
    {
        _publisher = publisher;
    }

    /// <inheritdoc/>
    public Task PublishAsync<TEvent>(TEvent eventData, CancellationToken cancellationToken = default)
    {
        string eventName = GetEventName(eventData);
        return _publisher.PublishAsync(eventName, eventData, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public Task PublishDelayAsync<TEvent>(TimeSpan delayTime, TEvent eventData, CancellationToken cancellationToken = default)
    {
        string eventName = GetEventName(eventData);
        return _publisher.PublishAsync(eventName, eventData, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 获取事件名称
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <param name="eventData">源事件</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    protected virtual string GetEventName<TEvent>(TEvent eventData)
    {
        string? eventName = typeof(TEvent).GetCustomAttribute<EventNameAttribute>()?.Name;
        if (string.IsNullOrWhiteSpace(eventName))
            throw new InvalidOperationException("Event name cannot be empty");

        return eventName;
    }
}
