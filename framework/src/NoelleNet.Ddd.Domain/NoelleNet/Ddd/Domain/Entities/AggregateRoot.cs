using NoelleNet.Ddd.Domain.Events;

namespace NoelleNet.Ddd.Domain.Entities;

/// <summary>
/// 聚合根基类
/// </summary>
/// <typeparam name="TIdentifier">实体标识符的数据类型</typeparam>
public class AggregateRoot<TIdentifier> : Entity<TIdentifier>, IAggregateRoot, IHasDomainEvents
{
    #region 领域事件
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <inheritdoc/>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// 添加一个领域事件
    /// </summary>
    /// <param name="eventData">待添加的领域事件</param>
    protected void AddDomainEvent(IDomainEvent eventData)
    {
        _domainEvents.Add(eventData);
    }

    /// <summary>
    /// 移除一个领域事件
    /// </summary>
    /// <param name="eventData">待移除的领域事件</param>
    protected void RemoveDomainEvent(IDomainEvent eventData)
    {
        _domainEvents.Remove(eventData);
    }

    /// <inheritdoc/>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
    #endregion
}
