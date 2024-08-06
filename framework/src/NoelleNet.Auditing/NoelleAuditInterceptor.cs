﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NoelleNet.Security;

namespace NoelleNet.Auditing.EntityFrameworkCore;

/// <summary>
/// 审计拦截器，用于在数据库操作（如保存更改）时设置实体审计信息
/// </summary>
/// <typeparam name="TUser"></typeparam>
/// <param name="currentUser">当前用户信息</param>
public class NoelleAuditInterceptor<TUser>(ICurrentUser currentUser) : SaveChangesInterceptor
{
    private readonly ICurrentUser _currentUser = currentUser;

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        SetAudit(eventData.Context);
        return base.SavedChanges(eventData, result);
    }

    public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        SetAudit(eventData.Context);
        return base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// 设置审计信息
    /// </summary>
    /// <param name="context"><see cref="DbContext"/> 实例</param>
    private void SetAudit(DbContext? context)
    {
        var entries = context?.ChangeTracker.Entries()
                                            .Where(e => e.Entity is ICreationAuditable<TUser> || e.Entity is IModificationAuditable<TUser>);
        if (entries == null)
            return;

        DateTime currentTime = DateTime.Now;
        var userId = _currentUser.Id == null ? default : _currentUser.Id.To<TUser>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                var creationAuditable = entry.Entity as ICreationAuditable<TUser>;
                creationAuditable?.SetCreationAudit(currentTime, userId!);
            }
            else if (entry.State == EntityState.Modified)
            {
                var modificationAuditable = entry.Entity as IModificationAuditable<TUser>;
                modificationAuditable?.UpdateModificationAudit(currentTime, userId!);
            }
        }
    }
}
