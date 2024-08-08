using MediatR;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace NoelleNet.EntityFrameworkCore.Interceptors;

/// <summary>
/// 在保存所有更改前，调度所有领域事件。在注册拦截器时，该拦截器需要放在其他 <see cref="SaveChangesInterceptor"/> 之后，确保是最后一个被调用
/// </summary>
/// <param name="mediator"><see cref="IMediator"/> 实例</param>
public class NoelleDomainEventInterceptor(IMediator mediator) : SaveChangesInterceptor
{
    private readonly IMediator _mediator = mediator;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context != null)
            _mediator.DispatchDomainEventsAsync(eventData.Context.ChangeTracker).GetAwaiter().GetResult();

        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context != null)
            await _mediator.DispatchDomainEventsAsync(eventData.Context.ChangeTracker, cancellationToken);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
