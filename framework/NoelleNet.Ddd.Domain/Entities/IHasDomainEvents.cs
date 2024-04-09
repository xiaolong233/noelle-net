using MediatR;

namespace NoelleNet.Ddd.Domain.Entities;

/// <summary>
/// 实现该接口的类表示支持领域事件
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>
    /// 返回当前实体的领域事件的只读集合
    /// </summary>
    IReadOnlyCollection<INotification> DomainEvents { get; }

    /// <summary>
    /// 添加一个领域事件项 
    /// </summary>
    /// <param name="eventItem">领域事件项</param>
    void AddDomainEvent(INotification eventItem);

    /// <summary>
    /// 删除一个领域事件项
    /// </summary>
    /// <param name="eventItem">领域事件项</param>
    void RemoveDomainEvent(INotification eventItem);

    /// <summary>
    /// 清空所有领域事件
    /// </summary>
    void ClearDomainEvents();
}
