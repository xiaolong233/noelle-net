using NoelleNet.Core.Exceptions;

namespace NoelleNet.Ddd.Domain.Exceptions;

/// <summary>
/// 实体未找到时的异常信息
/// </summary>
/// <param name="message">异常信息</param>
/// <param name="entityType">实体类型</param>
/// <param name="id">实体的标识符</param>
/// <param name="innerException">内部异常</param>
public class NoelleEntityNotFoundException(string message, Type entityType, object? id = default, Exception? innerException = default) : NoelleNotFoundException(message, innerException)
{

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="entityType">实体类型</param>
    /// <param name="id">实体的标识符</param>
    /// <param name="innerException">内部异常</param>
    public NoelleEntityNotFoundException(Type entityType, object? id = default, Exception? innerException = default)
        : this(id == null ? $"未指定实体的标识符。实体类型：{entityType.FullName}"
                          : $"未找到指定标识符的实体。实体类型：{entityType.FullName}，标识符：{id}",
               entityType, id, innerException)
    {

    }

    /// <summary>
    /// 实体类型
    /// </summary>
    public Type EntityType { get; init; } = entityType;

    /// <summary>
    /// 实体的标识符
    /// </summary>
    public object? Id { get; init; } = id;
}
