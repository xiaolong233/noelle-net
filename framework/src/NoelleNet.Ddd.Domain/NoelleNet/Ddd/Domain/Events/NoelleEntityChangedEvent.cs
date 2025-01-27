using MediatR;

namespace NoelleNet.Ddd.Domain.Events;

/// <summary>
/// 实体修改类型
/// </summary>
public enum EntityChangeType
{
    /// <summary>
    /// 创建
    /// </summary>
    Create,
    /// <summary>
    /// 更新
    /// </summary>
    Update,
    /// <summary>
    /// 删除
    /// </summary>
    Delete,
}

/// <summary>
/// 实体修改后触发的事件
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
/// <param name="Entity">实体实例</param>
/// <param name="ChangeType">修改类型</param>
public record NoelleEntityChangedEvent<TEntity>(TEntity Entity, EntityChangeType ChangeType) : INotification;
