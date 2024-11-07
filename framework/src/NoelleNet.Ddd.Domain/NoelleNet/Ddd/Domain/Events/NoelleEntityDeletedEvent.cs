using MediatR;

namespace NoelleNet.Ddd.Domain.Events;

/// <summary>
/// 实体删除后触发的事件
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
/// <param name="Entity">实体对象</param>
public record NoelleEntityDeletedEvent<TEntity>(TEntity Entity) : INotification;
