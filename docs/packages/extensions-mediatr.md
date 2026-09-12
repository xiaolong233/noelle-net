# NoelleNet.Extensions.MediatR

MediatR 管道行为：命令日志、事务自动管理 ✧

```bash
dotnet add package NoelleNet.Extensions.MediatR
```

命名空间：`NoelleNet.Extensions.MediatR`。本包依赖 [MediatR](https://github.com/jbogard/MediatR)、[NoelleNet.Uow](uow.md) 与 EF Core（事务行为要拿 `DbContext`）。

⚠️ 包内**没有** DI 注册扩展方法，行为需要你自己 `AddOpenBehavior` 注册：

```csharp
services.AddLocalEventBus(cfg =>
{
    cfg.RegisterServicesFromAssemblies(assemblies);
    cfg.UseMediatR(x =>
    {
        x.RegisterServicesFromAssemblies(assemblies);
        x.AddOpenBehavior(typeof(NoelleLoggingBehavior<,>));
        x.AddOpenBehavior(typeof(NoelleTransactionBehavior<,>));
    });
});
```

---

## NoelleTransactionBehavior

`IPipelineBehavior<TRequest, TResponse>`，让每个 MediatR 命令自带事务：

```csharp
public class NoelleTransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
```

构造依赖 `ILogger<...>`、`DbContext`、`IUnitOfWork`、`ITransactionManager`。执行流程：

1. 已有活动事务（`HasActiveTransaction`）→ 直接 `next()`。**嵌套命令复用外层事务**，不会重复开启；
2. 用 `DbContext.Database.CreateExecutionStrategy()` 包住整段（兼容 EF Core 的瞬时故障重试策略）；
3. `BeginAsync` → 记日志 → `next()` 处理命令 → `SaveChangesAsync` → `CommitAsync`；
4. 任一步骤抛异常 → 仍有活动事务则 `RollbackAsync` → 记错误日志 → 原样抛出。

因为第 3 步已经统一保存，**命令处理器里不需要自己调 `SaveChangesAsync`**：

```csharp
public class CreateTodoItemCommandHandler : IRequestHandler<CreateTodoItemCommand, Guid>
{
    public async Task<Guid> Handle(CreateTodoItemCommand request, CancellationToken cancellationToken)
    {
        var item = new TodoItem(request.Title);
        await repository.AddAsync(item, cancellationToken);
        return item.Id;      // 不用 SaveChanges，管道会保存并提交
    }
}
```

⚠️ 泛型参数是**非可空**的 `TResponse`（不是 `TResponse?`），这是刻意的：MediatR 按请求的实际响应类型精确闭合泛型，写成 `TResponse?` 会让值类型响应（如 `int`）解析不到本行为，导致事务被**静默跳过**。自定义行为时注意同样的坑。

`ITransactionManager` 是 Scoped，同一请求内共用同一事务实例——第 1 步与 `HasActiveTransaction` 正是为此服务。

---

## NoelleLoggingBehavior

`IPipelineBehavior<TRequest, TResponse>`，在命令前后各记一条 `Information` 日志，带命令名与请求 / 响应对象：

```csharp
_logger.LogInformation("命令开始 {CommandName} ({@Command})", request.GetGenericTypeName(), request);
```

命令名由 [NoelleNet.Core](core.md) 的 `GetGenericTypeName()` 生成。⚠️ 请求与响应对象会被**整体序列化进日志**，含敏感字段的命令要评估后再开启，或自行过滤。

---

## NoelleNoMediator

`IMediator` 的空白实现：所有 `Send` / `Publish` 直接返回 `Task.CompletedTask` 或 `default`，`CreateStream` 返回 `default!`。

它用于**不需要 MediatR 运行时的场景**（例如只想满足构造依赖、或测试里替掉真实中介者），注册后所有请求都不会被处理，且不会有任何提示——生产环境误用会表现为「命令发出去了但什么都没发生」。

---

## 相关包

- [NoelleNet.Uow](uow.md) — `IUnitOfWork` / `ITransactionManager` 契约
- [NoelleNet.EntityFrameworkCore](entity-framework-core.md) — 事务管理器的默认实现
- [NoelleNet.EventBus.Local.MediatR](event-bus-local-mediatr.md) — 行为的注册入口 `UseMediatR`
- [NoelleNet.Extensions.CAP.SqlServer](extensions-cap-sqlserver.md) — 换成事务发件箱
