using MediatR;

namespace NoelleNet.Ddd.Domain.Entities;

/// <summary>
/// 实体基类
/// </summary>
/// <typeparam name="TIdentifier">实体标识符的数据类型</typeparam>
public class Entity<TIdentifier> : IHasDomainEvents
{
    private int? _requestedHashCode;

    /// <summary>
    /// 实体标识符
    /// </summary>
    public virtual TIdentifier? Id { get; init; }

    /// <summary>
    /// 判断该实体是否为瞬时的
    /// </summary>
    /// <returns></returns>
    public bool IsTransient()
    {
        return EqualityComparer<TIdentifier>.Default.Equals(this.Id, default);
    }

    #region 重写
    public override bool Equals(object? obj)
    {
        if (obj == null || obj is not Entity<TIdentifier>)
            return false;

        if (Object.ReferenceEquals(this, obj))
            return true;

        if (this.GetType() != obj.GetType())
            return false;

        var entity = (Entity<TIdentifier>)obj;
        if (entity.IsTransient() || IsTransient())
            return false;

        return EqualityComparer<TIdentifier>.Default.Equals(this.Id, entity.Id);
    }

    public override int GetHashCode()
    {
        if (!IsTransient() && Id != null)
        {
            if (!_requestedHashCode.HasValue)
                _requestedHashCode = Id.GetHashCode() ^ 31;

            return _requestedHashCode.Value;
        }

        return base.GetHashCode();
    }

    public static bool operator ==(Entity<TIdentifier>? left, Entity<TIdentifier>? right)
    {
        if (Equals(left, null))
            return Equals(right, null);
        else
            return left.Equals(right);
    }

    public static bool operator !=(Entity<TIdentifier>? left, Entity<TIdentifier>? right)
    {
        return !(left == right);
    }
    #endregion

    #region 领域事件
    private readonly List<INotification> _domainEvents = [];

    /// <summary>
    /// 返回当前实体的领域事件的只读集合
    /// </summary>
    public IReadOnlyCollection<INotification> DomainEvents { get { return _domainEvents.AsReadOnly(); } }

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
