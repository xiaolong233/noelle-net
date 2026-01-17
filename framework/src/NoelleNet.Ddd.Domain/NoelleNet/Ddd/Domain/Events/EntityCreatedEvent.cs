using NoelleNet.Ddd.Domain.Entities;

namespace NoelleNet.Ddd.Domain.Events;

/// <summary>
/// 实体创建后触发的事件
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
/// <param name="Entity">实体实例</param>
public record EntityCreatedEvent<TEntity>(TEntity Entity) : IDomainEvent where TEntity : IEntity;
