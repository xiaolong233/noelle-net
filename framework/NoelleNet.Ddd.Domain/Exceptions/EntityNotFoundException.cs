namespace NoelleNet.Ddd.Domain.Exceptions;

/// <summary>
/// 实体未找到时的异常信息
/// </summary>
public class EntityNotFoundException : Exception
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="entityType">实体类型</param>
    /// <param name="id">实体的标识符</param>
    /// <param name="innerException">内部异常</param>
    public EntityNotFoundException(Type entityType, object? id, Exception? innerException)
        : base(id == null ? $"未指定实体的标识符。实体类型：{entityType.FullName}"
                          : $"未找到指定标识符的实体。实体类型：{entityType.FullName}，标识符：{id}", innerException)
    {
        EntityType = entityType;
        Id = id;
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="entityType">实体类型</param>
    public EntityNotFoundException(Type entityType) : this(entityType, null, null)
    {
    }

    /// <summary>
    /// 实体类型
    /// </summary>
    public Type EntityType { get; }

    /// <summary>
    /// 实体的标识符
    /// </summary>
    public object? Id { get; }
}
