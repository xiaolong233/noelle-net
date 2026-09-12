# NoelleNet.EntityFrameworkCore

EF Core 集成：仓储薄封装、工作单元、事务管理器，以及三个 `SaveChangesInterceptor` ✧

```bash
dotnet add package NoelleNet.EntityFrameworkCore
```

依赖 `Microsoft.EntityFrameworkCore.Relational` 10.0.12 与 [NoelleNet.Ddd.Domain](ddd-domain.md)、[NoelleNet.Uow](uow.md)。命名空间分散在 `NoelleNet.Ddd.Domain.Repositories.EntityFrameworkCore`、`NoelleNet.Uow`、`NoelleNet.EntityFrameworkCore.Interceptors`、`NoelleNet.Auditing.EntityFrameworkCore`。

---

## EfCoreRepository

```csharp
public class EfCoreRepository<TEntity, TDbContext> : IRepository<TEntity>
    where TEntity : IAggregateRoot
    where TDbContext : DbContext
{
    public EfCoreRepository(TDbContext dbContext);

    protected TDbContext DbContext { get; }
}
```

⚠️ **公开成员只有构造函数**——`DbContext` 属性是 `protected`，而且**没有任何 CRUD 方法**（没有 `GetDbSet()`、没有 `GetQueryable`、没有 `Insert`/`Update`/`Delete`）。

它做的是「把 `DbContext` 注入进来」这一件事，仓储的具体方法由你按聚合的用例自己写。所以派生类里的写法是直接用 `DbContext`：

```csharp
public class TodoItemRepository : EfCoreRepository<TodoItem, AppDbContext>, ITodoItemRepository
{
    public TodoItemRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<TodoItem> AddAsync(TodoItem item, CancellationToken cancellationToken = default)
    {
        var entry = await DbContext.TodoItems.AddAsync(item, cancellationToken);
        return entry.Entity;
    }

    public Task<TodoItem?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => DbContext.TodoItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public void Remove(TodoItem item) => DbContext.TodoItems.Remove(item);
}
```

仓储**没有自动注册**，记得显式加一行：

```csharp
services.AddScoped<ITodoItemRepository, TodoItemRepository>();
```

---

## UnitOfWork

```csharp
public class UnitOfWork : IUnitOfWork
{
    public UnitOfWork(DbContext dbContext);
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

纯透传 `_dbContext.SaveChangesAsync(...)`。

⚠️ 构造函数注入的是**抽象 `DbContext`**，所以注册时必须用 `AddDbContext<DbContext, AppDbContext>` 这个重载（同时注册抽象与具体两个服务类型），否则 `UnitOfWork` 解析不到：

```csharp
services.AddDbContext<DbContext, AppDbContext>((sp, options) =>
{
    options.UseSqlServer(connectionString);
    options.AddInterceptors(
        sp.GetRequiredService<NoelleAutoSetGuidKeyInterceptor>(),
        sp.GetRequiredService<NoelleAuditInterceptor>(),
        sp.GetRequiredService<NoelleDomainEventInterceptor>());
});
```

`UnitOfWork` **不开事务**，事务由 `ITransactionManager` / 命令管道负责。

---

## NoelleTransactionManager

`ITransactionManager` 的默认实现（仅 EF 事务）：

```csharp
public class NoelleTransactionManager : ITransactionManager
{
    public NoelleTransactionManager(DbContext dbContext);

    protected IDbContextTransaction? CurrentTransaction { get; set; }

    public virtual bool HasActiveTransaction { get; }
    public virtual Guid? TransactionId { get; }
    public virtual Task BeginAsync(CancellationToken cancellationToken = default);
    public virtual Task BeginAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default);
    public virtual Task CommitAsync(CancellationToken cancellationToken = default);
    public virtual Task RollbackAsync(CancellationToken cancellationToken = default);
}
```

关键点：

- `HasActiveTransaction` **只看本实例的 `CurrentTransaction`**，不查 `Database.CurrentTransaction` 或环境事务；
- 已有活动事务时 `BeginAsync` 抛 `InvalidOperationException`；没有活动事务时 `CommitAsync` / `RollbackAsync` 也抛；
- 提交 / 回滚后释放事务并置回 `null`；
- **`CommitAsync` 不调 `SaveChanges`**——保存是 `IUnitOfWork` 的事；
- 方法都是 `virtual`，可派生重写（CAP 版就是这么做的）。

⚠️ 需要「消息与数据原子提交」时替换为 CAP 事务发件箱实现，见 [NoelleNet.Extensions.CAP.SqlServer](extensions-cap-sqlserver.md)。

---

## 三个拦截器

顺序很重要，**`NoelleDomainEventInterceptor` 必须最后注册**（它的类注释明确要求排在其它 `SaveChangesInterceptor` 之后）：

```csharp
options.AddInterceptors(
    sp.GetRequiredService<NoelleAutoSetGuidKeyInterceptor>(),   // 1. 先补主键
    sp.GetRequiredService<NoelleAuditInterceptor>(),            // 2. 再填审计字段
    sp.GetRequiredService<NoelleDomainEventInterceptor>());     // 3. 最后派发领域事件

services.AddScoped<NoelleAutoSetGuidKeyInterceptor>();
services.AddScoped<NoelleAuditInterceptor>();
services.AddScoped<NoelleDomainEventInterceptor>();
```

因为 `AddDbContext` 的回调是**每次创建上下文时**执行的，拦截器从容器解析，所以三个都要单独注册（Scoped）。

### NoelleAutoSetGuidKeyInterceptor

构造依赖 `IGuidGenerator`（[NoelleNet.Core](core.md)）。**只处理 `Entity<Guid>`**：`Id == Guid.Empty` 时生成新值并写入，非空 `Id` 不覆盖。

```csharp
services.AddSingleton<IGuidGenerator, NoelleGuidGenerator>();
```

⚠️ 适用场景是主键配了 `ValueGeneratedNever` 的时候。EF 默认的键生成在 `DetectChanges` 阶段就赋好值了，那时拦截器看到的不再是 `Guid.Empty`，也就不会触发。它**不按 `EntityState` 过滤**，但只有空 `Guid` 才会被改，实际影响可控。

### NoelleAuditInterceptor

构造依赖 `ICurrentUser`（[NoelleNet.Core](core.md)）。`SavingChanges` 时填充 [NoelleNet.Auditing](auditing.md) 那四个字段：

| 状态 | 填充 |
|------|------|
| `Added` | `CreatedAt` = 当前时间、`CreatedBy` = `ICurrentUser.UserId` |
| `Modified` | `LastModifiedAt` = 当前时间、`LastModifiedBy` = `ICurrentUser.UserId` |
| `Deleted` / `Unchanged` | 不动 |

当前时间取自 `protected virtual DateTime GetNow()`，默认 **`DateTime.UtcNow`**（要用本地时间或可注入的时钟，派生重写 `GetNow()`）。新增时**不会**填 `LastModifiedAt`。

写入走 [NoelleNet.Core](core.md) 的 `NoelleObjectHelper.TrySetProperty`，属性不可写时静默跳过，不抛异常。

### NoelleDomainEventInterceptor

构造依赖 `ILocalEventBus`（[NoelleNet.EventBus](event-bus.md)）。派发时机是 **`SaveChanges` 提交之前、数据库事务之内**：

1. 收集 `ChangeTracker` 中所有 `IHasDomainEvents` 且事件非空的实体；
2. **先把事件快照成列表**；
3. **再清空**这些实体的事件集合；
4. 然后逐个 `await ILocalEventBus.PublishAsync(...)`，**顺序执行、不并行**。

顺序保证：实体按 `ChangeTracker.Entries()` 的顺序，实体内部按 `AddDomainEvent` 的调用顺序。先快照后清空意味着处理器里新挂的事件**不会**在本轮被派发。

处理器契约：

1. ✅ 可以增删改实体，改动随本次保存一起提交；
2. ❌ **禁止再调 `SaveChanges`**（嵌套保存会重复提交、状态不一致）；
3. 📤 短信、HTTP 调用等外部副作用请通过 `IDistributedEventBus` 发布集成事件；
4. 🚀 推荐异步 `SaveChangesAsync`——同步路径会 `Task.Run(...).GetAwaiter().GetResult()` 阻塞等待（源码注释说明是为规避自定义 `SynchronizationContext` 死锁）。

```csharp
public class TodoItemCreatedHandler : IDomainEventHandler<EntityCreatedEvent<TodoItem>>
{
    public Task HandleAsync(EntityCreatedEvent<TodoItem> eventData, CancellationToken cancellationToken = default)
    {
        // 可以改别的实体，别调 SaveChanges
        return Task.CompletedTask;
    }
}
```

---

## 完整注册示例

```csharp
// 数据库 + 拦截器
services.AddDbContext<DbContext, AppDbContext>((sp, options) =>
{
    options.UseSqlServer(connectionString);
    options.AddInterceptors(
        sp.GetRequiredService<NoelleAutoSetGuidKeyInterceptor>(),
        sp.GetRequiredService<NoelleAuditInterceptor>(),
        sp.GetRequiredService<NoelleDomainEventInterceptor>());
});
services.AddScoped<NoelleAutoSetGuidKeyInterceptor>();
services.AddScoped<NoelleAuditInterceptor>();
services.AddScoped<NoelleDomainEventInterceptor>();

// 工作单元与事务
services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddScoped<ITransactionManager, NoelleTransactionManager>();

// 仓储
services.AddScoped<ITodoItemRepository, TodoItemRepository>();
```

查询与分页可参考 [NoelleNet.Application.Contracts](application-contracts.md) 的 `PagingDto`。

---

## 相关包

- [NoelleNet.Ddd.Domain](ddd-domain.md) — 实体、聚合根、`IRepository` 标记接口
- [NoelleNet.Uow](uow.md) — `IUnitOfWork` / `ITransactionManager` 契约
- [NoelleNet.Auditing](auditing.md) — 审计字段契约
- [NoelleNet.Core](core.md) — `ICurrentUser`、`IGuidGenerator`
- [NoelleNet.Extensions.CAP.SqlServer](extensions-cap-sqlserver.md) — 事务发件箱
- [NoelleNet.Extensions.MediatR](extensions-mediatr.md) — 命令自动事务
