using NoelleNet.EventBus.Abstractions.Local;

namespace NoelleNet.Ddd.Domain.Events;

/// <summary>
/// 领域事件处理程序接口
/// </summary>
/// <typeparam name="TEvent">领域事件类型</typeparam>
public interface IDomainEventHandler<in TEvent> : ILocalEventHandler<TEvent> where TEvent : IDomainEvent
{
}
