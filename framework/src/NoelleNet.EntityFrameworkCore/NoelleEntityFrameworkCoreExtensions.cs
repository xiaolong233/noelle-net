using MediatR;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NoelleNet.Ddd.Domain.Entities;

namespace NoelleNet.EntityFrameworkCore;

public static class NoelleEntityFrameworkCoreExtensions
{
    /// <summary>
    /// 派发领域事件
    /// </summary>
    /// <param name="mediator"><see cref="IMediator"/> 实例</param>
    /// <param name="changeTracker"><see cref="ChangeTracker"/> 实例</param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    public static async Task DispatchDomainEventsAsync(this IMediator mediator, ChangeTracker changeTracker, CancellationToken cancellationToken = default)
    {
        // 获取所有发生更改，且含有领域事件的实体对象
        var entities = changeTracker.Entries<IHasDomainEvents>()
                                    .Where(e => e.Entity.DomainEvents != null && e.Entity.DomainEvents.Count > 0)
                                    .ToList();

        // 获取所有领域事件
        var domainEvents = entities.SelectMany(e => e.Entity.DomainEvents).ToList();

        // 清空实体里的领域事件
        entities.ForEach(e => e.Entity.ClearDomainEvents());

        // 发布领域事件
        foreach (var domainEvent in domainEvents)
        {
            await mediator.Publish(domainEvent, cancellationToken);
        }
    }
}
