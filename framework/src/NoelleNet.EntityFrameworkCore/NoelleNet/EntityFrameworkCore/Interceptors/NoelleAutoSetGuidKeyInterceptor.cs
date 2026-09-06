using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NoelleNet.Ddd.Domain.Entities;

namespace NoelleNet.EntityFrameworkCore.Interceptors;

/// <summary>
/// 自动设置实体标识符拦截器，当 <see cref="Entity{TIdentifier}"/> 的标识符为 <see cref="Guid"/> 类型并且为空时，自动设置一个新的 <see cref="Guid"/> 值。
/// 适用场景：键关闭了 EF 值生成（ValueGeneratedNever）的实体，期望由 <see cref="IGuidGenerator"/> 生成有序 GUID；
/// 默认配置下 EF 自带键生成会在 DetectChanges 阶段为 Guid 键赋值，拦截器不会触发（也不会覆盖已有值）。
/// </summary>
/// <param name="guidGenerator"><see cref="Guid"/> 的生成器</param>
public class NoelleAutoSetGuidKeyInterceptor(IGuidGenerator guidGenerator) : SaveChangesInterceptor
{
    private readonly IGuidGenerator _guidGenerator = guidGenerator ?? throw new ArgumentNullException(nameof(guidGenerator));

    /// <inheritdoc/>
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Handle(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc/>
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

            // 经 PropertyEntry 写入，保证 EF 变更跟踪器感知到键值变化
            var property = entry.Property(nameof(Entity<Guid>.Id));
            property.CurrentValue = _guidGenerator.Generate();
            property.IsTemporary = false;
        }
    }
}
