﻿using MediatR;
using Microsoft.EntityFrameworkCore;

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
        _mediator.DispatchDomainEventsAsync(ChangeTracker).Wait();
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
        await _mediator.DispatchDomainEventsAsync(ChangeTracker, cancellationToken);
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}
