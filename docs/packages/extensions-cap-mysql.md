# NoelleNet.Extensions.CAP.MySql

支持 CAP 事务发件箱的 `ITransactionManager` 实现（MySQL）✧

```bash
dotnet add package NoelleNet.Extensions.CAP.MySql
```

⚠️ **命名空间有坑**：类名是 `NoelleCapTransactionManager`，但命名空间是 **`NoelleNet.Uow`**（不是 `NoelleNet.Extensions.CAP.MySql`）。因为三个数据库包的类名完全相同、命名空间也相同，**同一个项目里只能引用其中一个**，否则类型冲突。

依赖 `DotNetCore.CAP.MySql` 10.0.2 与 [NoelleNet.EntityFrameworkCore](entity-framework-core.md)。

---

## 它解决什么

默认的 `NoelleTransactionManager` 只做 EF 事务：消息是通过 CAP 的 `ICapPublisher` 发的，若不在同一事务里，业务数据提交失败时消息可能已经出去了。

本包继承 `NoelleTransactionManager`，把 CAP 的事务发件箱挂到 EF 事务上——消息先写进发件箱表，与业务数据同一事务提交，从而保证「要么都成功，要么都失败」。

---

## 用法

把事务管理器替换掉即可，其他注册不变：

```csharp
services.AddScoped<IUnitOfWork, UnitOfWork>();
services.Replace(ServiceDescriptor.Scoped<ITransactionManager, NoelleCapTransactionManager>());
```

配 [NoelleNet.Extensions.MediatR](extensions-mediatr.md) 的 `NoelleTransactionBehavior` 后，命令管道自动走这条路径：开事务 → 处理命令（其间发的消息进发件箱）→ `SaveChangesAsync` → 提交。

---

## 实现要点

```csharp
public class NoelleCapTransactionManager : NoelleTransactionManager
{
    public NoelleCapTransactionManager(DbContext dbContext, ICapPublisher capPublisher) : base(dbContext)
}
```

- 构造依赖抽象 `DbContext` 与 CAP 的 `ICapPublisher`，两者为 `null` 时抛 `ArgumentNullException`；
- 重写两个 `BeginAsync` 重载，内部调用 `_dbContext.Database.BeginTransaction(isolationLevel, _capPublisher, false)` —— 第三个参数 `false` 表示**不自动提交**；
- 开事务用的是**同步** `BeginTransaction`（不是 `BeginTransactionAsync`），源码注释说明这是为了避免 CAP 的 `AsyncLocal` 在异步上下文中失效；
- 已有活动事务时抛 `InvalidOperationException`，`CommitAsync` / `RollbackAsync` / `Dispose` 沿用基类实现。

⚠️ 发件箱表由 CAP 维护，首次使用前需按 CAP 的方式初始化（执行建表脚本或调用其初始化 API）。MySQL 下还需确认发件箱存储所用的连接与 EF 的连接指向同一个库，否则事务无法覆盖消息表。

---

## 另外两个数据库

| 数据库 | 包 | 依赖 |
|--------|-----|------|
| SQL Server | [NoelleNet.Extensions.CAP.SqlServer](extensions-cap-sqlserver.md) | `DotNetCore.CAP.SqlServer` |
| PostgreSQL | [NoelleNet.Extensions.CAP.PostgreSql](extensions-cap-postgresql.md) | `DotNetCore.CAP.PostgreSql` |

三个包的实现代码逐字相同，差异只在依赖的 CAP 提供程序。

---

## 相关包

- [NoelleNet.Uow](uow.md) — `ITransactionManager` 契约
- [NoelleNet.EntityFrameworkCore](entity-framework-core.md) — `NoelleTransactionManager` 基类
- [NoelleNet.Extensions.MediatR](extensions-mediatr.md) — 自动事务的命令管道
- [NoelleNet.EventBus.Distributed.CAP](event-bus-distributed-cap.md) — CAP 事件总线
