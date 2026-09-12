# NoelleNet.EventBus.Local.MediatR

基于 [MediatR](https://github.com/jbogard/MediatR) 的本地（进程内）事件总线实现 ✧

```bash
dotnet add package NoelleNet.EventBus
dotnet add package NoelleNet.EventBus.Local.MediatR
```

关键命名空间有两个：`NoelleNet.EventBus.Local`（`AddLocalEventBus` 与 `UseMediatR`）和 `NoelleNet.EventBus.Local.MediatR`（实现类型）。

---

## 注册

`UseMediatR` 是 `LocalEventBusConfiguration` 的扩展方法，在 `AddLocalEventBus` 里调用：

```csharp
var assemblies = new[] { typeof(Program).Assembly };

services.AddLocalEventBus(cfg =>
{
    cfg.RegisterServicesFromAssemblies(assemblies);          // 扫描 ILocalEventHandler<> 实现
    cfg.UseMediatR(x => x.RegisterServicesFromAssemblies(assemblies));   // 交给 MediatR 注册
});
```

`UseMediatR(Action<MediatRServiceConfiguration>)` 依次做了三件事：

1. `AddScoped<ILocalEventBus, MediatRLocalEventBus>()` —— 注册总线本体；
2. `AddMediatR(configure)` —— 把配置委托透传给 MediatR；
3. `TryAddTransient<INotificationHandler<LocalEventAdapter>, LocalEventHandlerAdapter>()` —— 注册桥接处理器。

两个参数都不允许为 `null`。

⚠️ 第 3 步用的是 `TryAddTransient`：如果配置委托里没有让 MediatR 扫描到 `NoelleNet.EventBus.Local.MediatR` 这个程序集（`LocalEventHandlerAdapter` 在其中），那么**发布事件时不会有任何处理器被调用，也不会报错**。所以给 MediatR 的程序集清单要包含事件总线所在的程序集，最省事的做法是像上面那样把 `Program` 所在程序集登记进去，并确保它引用了本包。

想给 MediatR 命令加管道行为，在同一个委托里追加：

```csharp
cfg.UseMediatR(x =>
{
    x.RegisterServicesFromAssemblies(assemblies);
    x.AddOpenBehavior(typeof(NoelleLoggingBehavior<,>));       // 见 NoelleNet.Extensions.MediatR
    x.AddOpenBehavior(typeof(NoelleTransactionBehavior<,>));
});
```

---

## 发布

```csharp
public class TodoAppService(ILocalEventBus localEventBus)
{
    public Task NotifyAsync(TodoItem item, CancellationToken cancellationToken = default)
        => localEventBus.PublishAsync(new TodoCreatedEvent(item.Id, item.Title), cancellationToken);
}
```

`PublishAsync` 会把事件包一层 `LocalEventAdapter`（`INotification`），再交给 `IMediator.Publish`，由 `LocalEventHandlerAdapter` 找到该事件类型的全部 `ILocalEventHandler<TEvent>` 依次 `await`。传 `null` 事件会抛 `ArgumentNullException`。

处理器的写法：

```csharp
public class TodoCreatedEventHandler : ILocalEventHandler<TodoCreatedEvent>
{
    public Task HandleAsync(TodoCreatedEvent eventData, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
```

---

## 内部实现

需要替换或调试时可能会碰到这几个类型：

| 类型 | 作用 |
|------|------|
| `MediatRLocalEventBus` | `ILocalEventBus` 实现，构造依赖 `IMediator` |
| `LocalEventAdapter` | 事件包装成 MediatR 的 `INotification`，暴露 `SourceEvent` 与 `EventType` |
| `LocalEventHandlerAdapter` | `INotificationHandler<LocalEventAdapter>`，负责分发到实际的 `ILocalEventHandler<TEvent>` |

`LocalEventHandlerAdapter` 用表达式树编译出强类型调用委托并缓存在静态字典里，所以同一事件类型的后续派发不再走反射。分发是**串行 await**：一个处理器抛异常会中断后续处理器，异常向上冒泡给 `PublishAsync` 的调用方。

---

## 相关包

- [NoelleNet.EventBus](event-bus.md) — 抽象层与处理器扫描规则
- [NoelleNet.Extensions.MediatR](extensions-mediatr.md) — MediatR 管道行为（日志、事务）
- [NoelleNet.EventBus.Distributed.CAP](event-bus-distributed-cap.md) — 需要跨进程时
