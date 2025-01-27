using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NoelleNet.Security;

namespace NoelleNet.Auditing.EntityFrameworkCore;

/// <summary>
/// 审计拦截器，用于在数据库操作（如保存更改）时设置实体审计信息
/// </summary>
/// <typeparam name="TUser">用户标识符的类型</typeparam>
/// <param name="currentUser">当前用户</param>
public class NoelleAuditInterceptor<TUser>(ICurrentUser currentUser) : SaveChangesInterceptor
{
    private readonly ICurrentUser _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));

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
            .Where(e => e.Entity is ICreationAuditable<TUser> || e.Entity is IModificationAuditable<TUser>);

        if (entries == null || !entries.Any())
            return;

        DateTime currentTime = DateTime.Now;
        var userId = string.IsNullOrWhiteSpace(_currentUser.Id) ? default : _currentUser.Id.To<TUser>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added && entry.Entity is ICreationAuditable<TUser> creationAuditable)
            {
                NoelleObjectHelper.TrySetProperty(creationAuditable, s => s.CreatedAt, _ => currentTime);
                NoelleObjectHelper.TrySetProperty(creationAuditable, s => s.CreatedBy, _ => userId);
            }
            else if (entry.State == EntityState.Modified && entry.Entity is IModificationAuditable<TUser> modificationAuditable)
            {
                NoelleObjectHelper.TrySetProperty(modificationAuditable, s => s.LastModifiedAt, _ => currentTime);
                NoelleObjectHelper.TrySetProperty(modificationAuditable, s => s.LastModifiedBy, _ => userId);
            }
        }
    }
}
