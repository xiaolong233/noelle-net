using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NoelleNet.Ddd.Domain.Events;
using NoelleNet.EventBus.Abstractions.Local;

namespace NoelleNet.EntityFrameworkCore.Interceptors;

/// <summary>
/// 在保存所有更改前，调度所有领域事件。在注册拦截器时，该拦截器需要放在其他 <see cref="SaveChangesInterceptor"/> 之后，确保是最后一个被调用
/// </summary>
/// <remarks>
/// 领域事件在 <c>SaveChanges</c> 提交前派发，处理器在数据库事务内执行，约束见 <see cref="IDomainEventHandler{TEvent}"/>：
/// 处理器可以增删改实体（改动随本次保存一起提交），但禁止嵌套调用 <c>SaveChanges</c>，
/// 外部副作用应通过 <c>IDistributedEventBus</c> 发布集成事件。
/// </remarks>
public class NoelleDomainEventInterceptor : SaveChangesInterceptor
{
    private readonly ILocalEventBus _localEventBus;

    /// <summary>
    /// 创建一个新的 <see cref="NoelleDomainEventInterceptor"/> 实例
    /// </summary>
    /// <param name="localEventBus"><see cref="ILocalEventBus"/> 实例</param>
    /// <exception cref="ArgumentNullException"></exception>
    public NoelleDomainEventInterceptor(ILocalEventBus localEventBus)
    {
        _localEventBus = localEventBus ?? throw new ArgumentNullException(nameof(localEventBus));
    }

    /// <inheritdoc/>
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context != null)
        {
            // 通过 Task.Run 将派发调度到线程池再同步等待，
            // 避免在自定义 SynchronizationContext（如 WinForms/WPF/旧 ASP.NET）下同步等待引发死锁
            Task.Run(() => DispatchDomainEventsAsync(eventData.Context.ChangeTracker)).GetAwaiter().GetResult();
        }

        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc/>
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
            await _localEventBus.PublishAsync(domainEvent, cancellationToken);
        }
    }
}
