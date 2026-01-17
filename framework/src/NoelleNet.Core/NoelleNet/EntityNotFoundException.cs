namespace NoelleNet;

/// <summary>
/// 实体未找到时引发的异常
/// </summary>
public class EntityNotFoundException : NoelleNotFoundException
{
    /// <summary>
    /// 创建一个新的 <see cref="EntityNotFoundException"/> 实例
    /// </summary>
    public EntityNotFoundException()
    {

    }

    /// <summary>
    /// 创建一个新的 <see cref="EntityNotFoundException"/> 实例
    /// </summary>
    /// <param name="message">错误信息</param>
    /// <param name="innerException">内部异常对象</param>
    public EntityNotFoundException(string message, Exception? innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// 创建一个新的 <see cref="EntityNotFoundException"/> 实例
    /// </summary>
    /// <param name="message">错误信息</param>
    public EntityNotFoundException(string message) : this(message, null)
    {
    }

    /// <summary>
    /// 创建一个新的 <see cref="EntityNotFoundException"/> 实例
    /// </summary>
    /// <param name="entityType">实体类型</param>
    /// <param name="id">实体的标识符</param>
    /// <param name="innerException">内部异常</param>
    public EntityNotFoundException(Type entityType, object? id = default, Exception? innerException = default)
        : this(id == null ? $"未指定实体的标识符。实体类型：{entityType.FullName}"
                          : $"未找到指定标识符的实体。实体类型：{entityType.FullName}，标识符：{id}",
              innerException)
    {
        EntityType = entityType;
        Id = id;
    }

    /// <summary>
    /// 获取或设置实体类型
    /// </summary>
    public Type? EntityType { get; set; }

    /// <summary>
    /// 获取或设置实体标识符
    /// </summary>
    public object? Id { get; set; }
}
