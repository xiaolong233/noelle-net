using MediatR;

namespace NoelleNet.Ddd.Domain.Events;

/// <summary>
/// 实体更新后触发的事件
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
/// <param name="Entity"></param>
public record NoelleEntityUpdatedEvent<TEntity>(TEntity Entity) : INotification;