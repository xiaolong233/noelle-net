using MediatR;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NoelleNet.Ddd.Domain.Entities;

namespace NoelleNet.EntityFrameworkCore.Interceptors;

/// <summary>
/// 在保存所有更改前，调度所有领域事件。在注册拦截器时，该拦截器需要放在其他 <see cref="SaveChangesInterceptor"/> 之后，确保是最后一个被调用
/// </summary>
/// <param name="mediator"><see cref="IMediator"/> 实例</param>
public class NoelleDomainEventInterceptor(IMediator mediator) : SaveChangesInterceptor
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context != null)
            DispatchDomainEventsAsync(eventData.Context.ChangeTracker).GetAwaiter().GetResult();

        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context != null)
            await DispatchDomainEventsAsync(eventData.Context.ChangeTracker, cancellationToken);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// 派发领域事件
    /// </summary>
    /// <param name="changeTracker"><see cref="ChangeTracker"/> 实例</param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    private async Task DispatchDomainEventsAsync(ChangeTracker changeTracker, CancellationToken cancellationToken = default)
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
            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }
}
