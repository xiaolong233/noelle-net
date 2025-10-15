namespace NoelleNet.Ddd.Domain.Entities;

/// <summary>
/// 实体基类
/// </summary>
public abstract class Entity : IEntity
{
    public abstract object?[] GetIdentifiers();

    /// <summary>
    /// 判断实体是否为瞬态
    /// </summary>
    /// <returns></returns>
    public virtual bool IsTransient()
    {
        var identifiers = GetIdentifiers();
        if (identifiers == null || identifiers.Length <= 0)
            return true;

        return identifiers.Any(id => id switch
        {
            null => true,
            Guid guid when guid == Guid.Empty => true,
            int i when i == 0 => true,
            long l when l == 0L => true,
            string s when string.IsNullOrWhiteSpace(s) => true,
            _ => false
        });
    }

    public override string ToString()
    {
        var identifiers = GetIdentifiers();
        var idString = identifiers.Length switch
        {
            0 => "[No Identifiers]",
            1 => $"[Id: {identifiers[0] ?? "null"}]",
            _ => $"[Ids: {string.Join(", ", identifiers.Select(x => x ?? "null"))}]"
        };

        return $"{GetType().Name} {idString}";
    }

    #region 重载运算符
    public override bool Equals(object? obj)
    {
        return Equals(obj as Entity);
    }

    public virtual bool Equals(Entity? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        var currentIsTransient = IsTransient();
        var otherIsTransient = other.IsTransient();

        // 两个都是瞬态实体，仅当引用相同时相等
        if (currentIsTransient && otherIsTransient)
            return ReferenceEquals(this, other);

        // 一个瞬态、一个非瞬态 => 一定不相等
        if (currentIsTransient || otherIsTransient)
            return false;

        // 均非瞬态 => 比较标识符
        var currentIds = GetIdentifiers();
        var otherIds = other.GetIdentifiers();

        if (currentIds.Length != otherIds.Length)
            return false;

        for (int i = 0; i < currentIds.Length; i++)
        {
            if (!Equals(currentIds[i], otherIds[i]))
                return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        var identifiers = GetIdentifiers();
        if (identifiers == null || identifiers.Length == 0)
            return base.GetHashCode();

        var hash = new HashCode();
        foreach (var id in identifiers)
        {
            hash.Add(id);
        }

        return hash.ToHashCode();
    }

    public static bool operator ==(Entity? left, Entity? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(Entity? left, Entity? right)
    {
        return !(left == right);
    }
    #endregion
}

/// <summary>
/// 实体基类，实现了 <see cref="IEntity{TIdentifier}"/> 接口
/// </summary>
/// <typeparam name="TIdentifier">实体标识符的数据类型</typeparam>
public abstract class Entity<TIdentifier> : Entity, IEntity<TIdentifier>
{
    /// <summary>
    /// 创建一个新的 <see cref="Entity{TIdentifier}"/> 实例
    /// </summary>
    protected Entity() { }

    /// <summary>
    /// 创建一个新的 <see cref="Entity{TIdentifier}"/> 实例
    /// </summary>
    /// <param name="id">实体的标识符</param>
    protected Entity(TIdentifier id)
    {
        Id = id;
    }

    public virtual TIdentifier Id { get; protected set; } = default!;

    public override object?[] GetIdentifiers()
    {
        return [Id];
    }
}
