namespace NoelleNet.EventBus.Distributed;

/// <summary>
/// 分布式事件总线选项
/// </summary>
public class NoelleDistributedEventBusOptions
{
    /// <summary>
    /// 分布式事件处理器与事件类型的配对集合
    /// </summary>
    public IReadOnlyList<(Type HandlerType, Type EventType)> HandlerEventTypePairs { get; internal set; } = [];
}
