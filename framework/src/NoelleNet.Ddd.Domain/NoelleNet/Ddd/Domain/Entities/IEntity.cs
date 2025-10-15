namespace NoelleNet.Ddd.Domain.Entities;

/// <summary>
/// 定义一个实体
/// </summary>
public interface IEntity
{
    /// <summary>
    /// 返回实体的标识符数组
    /// </summary>
    /// <returns></returns>
    object?[] GetIdentifiers();
}

/// <summary>
/// 定义一个具有 Id 属性的单个标识符的实体
/// </summary>
/// <typeparam name="TIdentifier"></typeparam>
public interface IEntity<TIdentifier> : IEntity
{
    /// <summary>
    /// 实体的标识符
    /// </summary>
    TIdentifier Id { get; }
}
