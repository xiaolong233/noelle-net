using NoelleNet.Ddd.Domain.Events;

namespace NoelleNet.Ddd.Domain.Entities;

/// <summary>
/// 定义一个表示聚合根的实体
/// </summary>
public abstract class AggregateRoot : Entity, IAggregateRoot, IHasDomainEvents
{
    #region 领域事件
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <inheritdoc/>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <inheritdoc/>
    protected void AddDomainEvent(IDomainEvent eventData)
    {
        _domainEvents.Add(eventData);
    }

    /// <inheritdoc/>
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

/// <summary>
/// 定义一个表示聚合根的实体
/// </summary>
/// <typeparam name="TIdentifier">实体标识符的数据类型</typeparam>
public abstract class AggregateRoot<TIdentifier> : Entity<TIdentifier>, IAggregateRoot, IHasDomainEvents
{
    /// <summary>
    /// 创建一个新的 <see cref="AggregateRoot{TIdentifier}"/> 实例
    /// </summary>
    protected AggregateRoot() { }

    /// <summary>
    /// 创建一个新的 <see cref="AggregateRoot{TIdentifier}"/> 实例
    /// </summary>
    /// <param name="id">实体的标识符</param>
    protected AggregateRoot(TIdentifier id) : base(id)
    {
    }

    #region 领域事件
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <inheritdoc/>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <inheritdoc/>
    protected void AddDomainEvent(IDomainEvent eventData)
    {
        _domainEvents.Add(eventData);
    }

    /// <inheritdoc/>
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
