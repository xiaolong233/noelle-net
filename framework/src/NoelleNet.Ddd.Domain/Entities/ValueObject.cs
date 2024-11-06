namespace NoelleNet.Ddd.Domain.Entities;

/// <summary>
/// 值对象基类
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// 比较两个值对象是否相等
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null ^ right is null)
            return false;
        return left is null || left.Equals(right);
    }

    /// <summary>
    /// 比较两个值对象是否不相等
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// 获取用于比较两个值对象实例是否相等的元素
    /// </summary>
    /// <returns></returns>
    protected abstract IEnumerable<object> GetEqualityCompareItems();

    /// <summary>
    /// 确定指定对象是否等于当前对象
    /// </summary>
    /// <param name="obj">要与当前对象进行比较的对象</param>
    /// <returns>如果指定的对象是等于当前对象，则为 true；否则为 false</returns>
    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;

        return GetEqualityCompareItems().SequenceEqual(((ValueObject)obj).GetEqualityCompareItems());
    }

    /// <summary>
    /// 获取当前值对象的哈希代码
    /// </summary>
    /// <returns>当前值对象的哈希代码</returns>
    public override int GetHashCode()
    {
        return GetEqualityCompareItems().Select(x => x != null ? x.GetHashCode() : 0)
                                        .Aggregate((x, y) => x ^ y);
    }

    /// <summary>
    /// 创建当前值对象的浅拷贝对象
    /// </summary>
    /// <returns>当前值对象的浅拷贝对象</returns>
    public ValueObject Copy()
    {
        return (ValueObject)MemberwiseClone();
    }
}

