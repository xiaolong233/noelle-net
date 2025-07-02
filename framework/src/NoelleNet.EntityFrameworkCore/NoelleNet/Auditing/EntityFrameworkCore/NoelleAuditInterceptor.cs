﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NoelleNet.Security;

namespace NoelleNet.Auditing.EntityFrameworkCore;

/// <summary>
/// 审计拦截器，用于在数据库操作（如保存更改）时设置实体审计信息
/// </summary>
public class NoelleAuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// 创建一个新的 <see cref="NoelleAuditInterceptor"/> 实例
    /// </summary>
    /// <param name="currentUser">当前用户信息</param>
    /// <exception cref="ArgumentNullException"></exception>
    public NoelleAuditInterceptor(ICurrentUser currentUser)
    {
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        SetAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        SetAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// 设置审计信息
    /// </summary>
    /// <param name="context">数据库上下文</param>
    protected virtual void SetAudit(DbContext? context)
    {
        var entries = context?.ChangeTracker
            .Entries()
            .Where(e => e.Entity is IHasCreatedAt || e.Entity is IMayHaveCreator || e.Entity is IModificationAudited);

        if (entries == null || !entries.Any())
            return;

        DateTime currentTime = DateTime.Now;
        Guid? userId = string.IsNullOrWhiteSpace(_currentUser.Id) ? null : Guid.Parse(_currentUser.Id);
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity is IHasCreatedAt hasCreatedAt)
                    NoelleObjectHelper.TrySetProperty(hasCreatedAt, s => s.CreatedAt, _ => currentTime);
                if (entry.Entity is IMayHaveCreator mayHaveCreator)
                    NoelleObjectHelper.TrySetProperty(mayHaveCreator, s => s.CreatedBy, _ => userId);
            }
            else if (entry.State == EntityState.Modified && entry.Entity is IModificationAudited modificationAuditable)
            {
                NoelleObjectHelper.TrySetProperty(modificationAuditable, s => s.LastModifiedAt, _ => currentTime);
                NoelleObjectHelper.TrySetProperty(modificationAuditable, s => s.LastModifiedBy, _ => userId);
            }
        }
    }
}

/// <summary>
/// 审计拦截器，用于在数据库操作（如保存更改）时设置实体审计信息
/// </summary>
/// <typeparam name="TOperatorId">操作人标识符的数据类型</typeparam>
public class NoelleAuditInterceptor<TOperatorId> : SaveChangesInterceptor
{
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// 创建一个新的 <see cref="NoelleAuditInterceptor{TOperatorId}"/> 实例
    /// </summary>
    /// <param name="currentUser">当前用户信息</param>
    /// <exception cref="ArgumentNullException"></exception>
    public NoelleAuditInterceptor(ICurrentUser currentUser)
    {
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        SetAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        SetAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// 设置审计信息
    /// </summary>
    /// <param name="context">数据库上下文</param>
    private void SetAudit(DbContext? context)
    {
        var entries = context?.ChangeTracker
            .Entries()
            .Where(e => e.Entity is IHasCreatedAt || e.Entity is IMayHaveCreator<TOperatorId> || e.Entity is IModificationAudited<TOperatorId>);

        if (entries == null || !entries.Any())
            return;

        DateTime currentTime = DateTime.Now;
        var userId = string.IsNullOrWhiteSpace(_currentUser.Id) ? default : _currentUser.Id.To<TOperatorId>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity is IHasCreatedAt hasCreatedAt)
                    NoelleObjectHelper.TrySetProperty(hasCreatedAt, s => s.CreatedAt, _ => currentTime);
                if (entry.Entity is IMayHaveCreator<TOperatorId> mayHaveCreator)
                    NoelleObjectHelper.TrySetProperty(mayHaveCreator, s => s.CreatedBy, _ => userId);
            }
            else if (entry.State == EntityState.Modified && entry.Entity is IModificationAudited<TOperatorId> modificationAuditable)
            {
                NoelleObjectHelper.TrySetProperty(modificationAuditable, s => s.LastModifiedAt, _ => currentTime);
                NoelleObjectHelper.TrySetProperty(modificationAuditable, s => s.LastModifiedBy, _ => userId);
            }
        }
    }
}
