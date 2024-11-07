using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NoelleNet.Ddd.Domain.Entities;

namespace NoelleNet.EntityFrameworkCore.Interceptors;

/// <summary>
/// 拦截器，当 <see cref="Entity{TIdentifier}"/> 的标识符为 <see cref="Guid"/> 类型并且为空时，自动设置一个新的 <see cref="Guid"/> 值。
/// </summary>
/// <param name="guidGenerator"><see cref="Guid"/> 的生成器</param>
public class NoelleAutoSetGuidKeyInterceptor(IGuidGenerator guidGenerator) : SaveChangesInterceptor
{
    private readonly IGuidGenerator _guidGenerator = guidGenerator;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Handle(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Handle(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Handle(DbContext? context)
    {
        if (context == null)
            return;

        foreach (var entry in context.ChangeTracker.Entries<Entity<Guid>>())
        {
            if (entry.Entity.Id != Guid.Empty)
                continue;

            var entityType = entry.Entity.GetType();
            var idProperty = entityType.GetProperty(nameof(entry.Entity.Id));
            idProperty?.SetValue(entry.Entity, _guidGenerator.Generate());
        }
    }
}
