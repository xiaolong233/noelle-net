using System.Text.Encodings.Web;
using System.Text.Json;

namespace NoelleNet.Ddd.Domain.Entities;

/// <summary>
/// 值对象基类
/// </summary>
public abstract class ValueObject
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.General) { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    #region 运算符重载
    /// <inheritdoc/>
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <inheritdoc/>
    public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);
    #endregion

    /// <inheritdoc/>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var item in GetEqualityComponents())
        {
            hash.Add(item);
        }
        return hash.ToHashCode();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        // 使用运行时类型序列化：若按编译期类型（ValueObject 基类，无公开属性）序列化，输出将恒为 "{}"
        return JsonSerializer.Serialize(this, GetType(), _jsonOptions);
    }
}

