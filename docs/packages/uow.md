# NoelleNet.Uow

工作单元与事务管理器的**接口**。实现不在这个包里 ✧

```bash
dotnet add package NoelleNet.Uow
```

命名空间：`NoelleNet.Uow`。本包只声明两个接口，实际实现由 [NoelleNet.EntityFrameworkCore](entity-framework-core.md) 提供：

| 接口 | 实现在哪里 |
|------|-----------|
| `IUnitOfWork` | `UnitOfWork`，位于 [NoelleNet.EntityFrameworkCore](entity-framework-core.md) |
| `ITransactionManager` | `NoelleTransactionManager`（仅 EF 事务），位于 [NoelleNet.EntityFrameworkCore](entity-framework-core.md)；CAP 事务发件箱版本位于 `NoelleNet.Extensions.CAP.*` |

平时只需引用实现包，把本包当作「契约在哪」的索引即可。

---

## IUnitOfWork

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

只做一件事：把当前 `DbContext` 的变更落库。默认实现 `UnitOfWork` 是**纯透传** `_dbContext.SaveChangesAsync(...)`——不开事务、不派发领域事件、不做任何加工，领域事件由 EF Core 拦截器负责。

---

## ITransactionManager

```csharp
public interface ITransactionManager : IDisposable, IAsyncDisposable
{
    bool HasActiveTransaction { get; }
    Guid? TransactionId { get; }

    Task BeginAsync(CancellationToken cancellationToken = default);
    Task BeginAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
```

- `HasActiveTransaction` —— **只看本实例持有的 `CurrentTransaction` 是否为 `null`**，不查询 `Database.CurrentTransaction` 或环境事务。因为 `ITransactionManager` 是 Scoped，这个标记在「同一请求」范围内有效，命令管道靠它避免嵌套命令重复开事务；
- `TransactionId` —— 当前事务标识，未开启时为 `null`；
- `BeginAsync` 有隔离级别重载（`System.Data.IsolationLevel`）；**已有活动事务时再调用会抛 `InvalidOperationException`**；`CommitAsync` / `RollbackAsync` 在没有活动事务时也抛 `InvalidOperationException`；
- 提交或回滚后会释放事务并把 `CurrentTransaction` 置回 `null`，所以可以接着开下一个；
- 类型本身实现 `IDisposable` / `IAsyncDisposable`，按 Scoped 注册，由容器负责释放。

---

## 怎么用

注册（按 Scoped）：

```csharp
services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddScoped<ITransactionManager, NoelleTransactionManager>();
```

日常两条路：

- **命令走事务** —— 配 `NoelleTransactionBehavior`（[NoelleNet.Extensions.MediatR](extensions-mediatr.md)），管道自动完成「开事务 → 处理 → `SaveChangesAsync` → 提交 / 回滚」，处理器里不用自己调；
- **手动控制** —— 在应用服务里直接注入 `IUnitOfWork` 调 `SaveChangesAsync()`。需要自己开事务时注入 `ITransactionManager`：

```csharp
await transactionManager.BeginAsync();
try
{
    await repository.AddAsync(item);
    await unitOfWork.SaveChangesAsync();
    await transactionManager.CommitAsync();
}
catch
{
    await transactionManager.RollbackAsync();
    throw;
}
```

⚠️ `ITransactionManager` 是 Scoped，同一请求内共用同一个事务实例。已经有活动事务时不要再 `BeginAsync`，先看 `HasActiveTransaction`。

需要「消息与数据原子提交」时，把事务管理器换成 CAP 事务发件箱实现：

```csharp
services.AddScoped<IUnitOfWork, UnitOfWork>();
services.Replace(ServiceDescriptor.Scoped<ITransactionManager, NoelleCapTransactionManager>());
```

三个数据库各有一个实现包，详见 [NoelleNet.Extensions.CAP.SqlServer](extensions-cap-sqlserver.md)（另有 [MySql](extensions-cap-mysql.md) / [PostgreSql](extensions-cap-postgresql.md)）。

---

## 相关包

- [NoelleNet.EntityFrameworkCore](entity-framework-core.md) — `UnitOfWork` 与 `NoelleTransactionManager`
- [NoelleNet.Extensions.MediatR](extensions-mediatr.md) — `NoelleTransactionBehavior` 自动事务
- [NoelleNet.Extensions.CAP.SqlServer](extensions-cap-sqlserver.md) — CAP 事务发件箱实现
