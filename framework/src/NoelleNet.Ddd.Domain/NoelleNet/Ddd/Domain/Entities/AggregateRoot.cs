using MediatR;

namespace NoelleNet.Ddd.Domain.Entities;

/// <summary>
/// 聚合根基类
/// </summary>
/// <typeparam name="TIdentifier">实体标识符的数据类型</typeparam>
public class AggregateRoot<TIdentifier> : Entity<TIdentifier>, IAggregateRoot, IHasDomainEvents
{
    #region 领域事件
    private readonly List<INotification> _domainEvents = [];

    /// <summary>
    /// 返回当前实体的领域事件的只读集合
    /// </summary>
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// 添加一个领域事件项 
    /// </summary>
    /// <param name="eventItem">领域事件项</param>
    public void AddDomainEvent(INotification eventItem)
    {
        _domainEvents.Add(eventItem);
    }

    /// <summary>
    /// 删除一个领域事件项
    /// </summary>
    /// <param name="eventItem">领域事件项</param>
    public void RemoveDomainEvent(INotification eventItem)
    {
        _domainEvents.Remove(eventItem);
    }

    /// <summary>
    /// 清空所有领域事件
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
    #endregion
}
