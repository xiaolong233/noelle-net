using MediatR;
using Microsoft.EntityFrameworkCore;
using NoelleNet.Ddd.Domain.Entities;

namespace NoelleNet.EntityFrameworkCore;

/// <summary>
/// 应用的数据库上下文
/// </summary>
/// <param name="options">要使用的 <see cref="DbContext"/> 选项</param>
/// <param name="mediator">本地事件总线</param>
public class NoelleDbContext(DbContextOptions options, IMediator mediator) : DbContext(options)
{
    private readonly IMediator _mediator = mediator;

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        DispatchDomainEventsAsync().Wait();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess"></param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(cancellationToken);
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// 派发领域事件
    /// </summary>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken = default)
    {
        // 获取所有发生更改，且含有领域事件的实体对象
        var entities = ChangeTracker.Entries<IHasDomainEvents>()
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
