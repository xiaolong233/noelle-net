namespace NoelleNet.EventBus.Abstractions;

/// <summary>
/// 事件总线接口
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// 发布一个事件
    /// </summary>
    /// <typeparam name="TEvent">事件类型</typeparam>
    /// <param name="eventData">待发布的事件</param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    Task PublishAsync<TEvent>(TEvent eventData, CancellationToken cancellationToken = default);
}
