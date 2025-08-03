namespace NoelleNet.EventBus.Abstractions.Distributed;

/// <summary>
/// 分布式事件处理器接口
/// </summary>
/// <typeparam name="TEvent">事件类型</typeparam>
public interface IDistributedEventHandler<in TEvent> : IEventHandler
{
    /// <summary>
    /// 开始处理事件
    /// </summary>
    /// <param name="eventData">待处理的事件</param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    Task HandleAsync(TEvent eventData, CancellationToken cancellationToken = default);
}
