using MediatR;

namespace NoelleNet.EventBus.Local.MediatR;

/// <summary>
/// 本地事件适配器
/// </summary>
public class LocalEventAdapter : INotification
{
    /// <summary>
    /// 创建一个新的 <see cref="LocalEventAdapter"/> 实例
    /// </summary>
    /// <param name="sourceEvent">源事件</param>
    /// <exception cref="ArgumentNullException"></exception>
    public LocalEventAdapter(object sourceEvent)
    {
        SourceEvent = sourceEvent ?? throw new ArgumentNullException(nameof(sourceEvent));
        EventType = sourceEvent.GetType();
    }

    /// <summary>
    /// 源事件
    /// </summary>
    public object SourceEvent { get; }

    /// <summary>
    /// 事件类型
    /// </summary>
    public Type EventType { get; }
}
