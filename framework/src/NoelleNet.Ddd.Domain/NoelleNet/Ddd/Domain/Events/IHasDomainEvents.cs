namespace NoelleNet.Ddd.Domain.Events;

/// <summary>
/// 实现该接口的类表示支持领域事件
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>
    /// 返回当前实体的领域事件的只读集合
    /// </summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// 清空所有领域事件
    /// </summary>
    void ClearDomainEvents();
}
