namespace NoelleNet.Ddd.Domain.Entities;

/// <summary>
/// 实体基类
/// </summary>
/// <typeparam name="TIdentifier">实体标识符的数据类型</typeparam>
public class Entity<TIdentifier> : IEntity
{
    private int? _requestedHashCode;

    /// <summary>
    /// 实体标识符
    /// </summary>
    public virtual TIdentifier? Id { get; protected set; }

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
}
