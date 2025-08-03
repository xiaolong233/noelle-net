namespace NoelleNet.EventBus.Abstractions.Distributed;

/// <summary>
/// 分布式事件总线接口
/// </summary>
public interface IDistributedEventBus : IEventBus
{
    /// <summary>
    /// 延迟发布一个事件
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <param name="delayTime">延迟时间</param>
    /// <param name="eventData">待发布的事件</param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    Task PublishDelayAsync<TEvent>(TimeSpan delayTime, TEvent eventData, CancellationToken cancellationToken = default);
}
