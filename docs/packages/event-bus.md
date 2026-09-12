# NoelleNet.EventBus

事件总线的**抽象层**：本地与分布式两套接口、事件名特性、处理器自动注册。实现分别在 `NoelleNet.EventBus.Local.MediatR` 与 `NoelleNet.EventBus.Distributed.CAP` ✧

```bash
dotnet add package NoelleNet.EventBus
```

命名空间：根为 `NoelleNet.EventBus.Abstractions`（另有 `.Local`、`.Distributed` 子命名空间），`AddLocalEventBus` / `AddDistributedEventBus` 分别在 `NoelleNet.EventBus.Local` / `NoelleNet.EventBus.Distributed`。

⚠️ 本包**没有可用的总线实现**——只声明接口与注册逻辑，不装 `Local.MediatR` 或 `Distributed.CAP` 的话容器里解析不到 `ILocalEventBus` / `IDistributedEventBus`。

---

## 接口

| 接口 | 成员 |
|------|------|
| `IEventBus` | `Task PublishAsync<TEvent>(TEvent eventData, CancellationToken cancellationToken = default)` |
| `IEventHandler` | 空标记接口 |
| `ILocalEventBus` | 继承 `IEventBus`，无新增成员 |
| `IDistributedEventBus` | 继承 `IEventBus`，另加 `Task PublishDelayAsync<TEvent>(TimeSpan delayTime, TEvent eventData, CancellationToken cancellationToken = default)` |
| `ILocalEventHandler<in TEvent>` | `Task HandleAsync(TEvent eventData, CancellationToken cancellationToken = default)` |
| `IDistributedEventHandler<in TEvent>` | 同上 |

另有 `NoelleDistributedEventBusOptions`，暴露扫描结果 `HandlerEventTypePairs`（`IReadOnlyList<(Type HandlerType, Type EventType)>`）。它的 setter 是 `internal`，只能在 `AddDistributedEventBus` 内部写入，消费方（如 CAP 的消费者选择器）只读。

两个处理器接口都是**逆变**（`in TEvent`），所以 `ILocalEventHandler<object>` 能接住任意事件——注册时框架正是按精确事件类型闭合泛型来查找的。

```csharp
public class TodoCreatedEventHandler : ILocalEventHandler<TodoCreatedEvent>
{
    public Task HandleAsync(TodoCreatedEvent eventData, CancellationToken cancellationToken = default)
    {
        // 处理进程内事件
        return Task.CompletedTask;
    }
}
```

---

## EventNameAttribute

分布式事件靠它确定事件名，标在**事件类型**上：

```csharp
[EventName("todo.created", Group = "todo-service")]
public record TodoCreatedEvent(Guid TodoId, string Title);
```

| 成员 | 说明 |
|------|------|
| `Name` | 事件名，构造函数传入，空白字符串抛 `ArgumentNullException` |
| `Group` | 分组名，可空，默认 `null` |
| `AttributeUsage` | `AttributeTargets.Class` |

本地事件不需要它。分布式事件**必须**有：发不出去或收不到时，先检查这里。

---

## 注册

两个入口都会扫描指定程序集，把处理器自动注册进容器：

```csharp
var assemblies = new[] { typeof(Program).Assembly };

// 本地：需要配合实现包使用（见下）
services.AddLocalEventBus(cfg => cfg.RegisterServicesFromAssemblies(assemblies));

// 分布式：需要配合实现包使用（见下）
services.AddDistributedEventBus(cfg => cfg.RegisterServicesFromAssemblies(assemblies));
```

`LocalEventBusConfiguration` / `DistributedEventBusConfiguration` 暴露同样的注册方法：

| 成员 | 说明 |
|------|------|
| `Services` | 当前 `IServiceCollection` |
| `AssembliesToRegister` | 已登记的程序集（只读） |
| `RegisterServicesFromAssembly(Assembly)` | 登记单个程序集，可链式调用 |
| `RegisterServicesFromAssemblies(params Assembly[])` | 登记多个程序集，可链式调用 |

### 扫描规则

- 找出所有**非抽象、非接口**的类型，取其实现的 `ILocalEventHandler<>` / `IDistributedEventHandler<>` 泛型接口，按**接口闭合的事件类型**注册；
- 程序集里的类型若加载失败会被跳过，不会因为一个坏类型让应用启动失败；
- 两边的注册语义**不同**，容易踩坑：

| | 本地 | 分布式 |
|---|---|---|
| 注册方式 | `TryAddEnumerable` + `Transient` | `TryAddTransient` |
| 同一事件类型的多个处理器 | **全部生效**，依次调用 | ⚠️ **只有扫描到的第一个生效**，其余被静默忽略 |

分布式事件想要一个事件触发多个消费者，得靠**消息中间件的订阅机制**（同一个 topic 的多个分组各自消费），而不是在同一进程里注册多个处理器。

⚠️ 两个注册方法的空值处理也不一致：`AddLocalEventBus` 的配置委托不可为空（传 `null` 抛 `ArgumentNullException`），`AddDistributedEventBus` 则既不校验 `services` 也不校验委托（`configure?.Invoke`），传 `null` 等于只做空扫描。

只在配置里登记的程序集才会被扫描——**处理器所在程序集忘了登记，就不会被注册**，且不会有任何提示。

---

## 实现包

| 实现 | 包 | 事件总线 |
|------|-----|----------|
| 进程内 | [NoelleNet.EventBus.Local.MediatR](event-bus-local-mediatr.md) | `ILocalEventBus` |
| 跨进程 | [NoelleNet.EventBus.Distributed.CAP](event-bus-distributed-cap.md) | `IDistributedEventBus` |

领域事件（`IDomainEvent`）走的是另一条路——由 EF Core 拦截器在 `SaveChanges` 前派发，见 [NoelleNet.Ddd.Domain](ddd-domain.md) 与 [NoelleNet.EntityFrameworkCore](entity-framework-core.md)。

---

## 相关包

- [NoelleNet.EventBus.Local.MediatR](event-bus-local-mediatr.md) — 本地实现
- [NoelleNet.EventBus.Distributed.CAP](event-bus-distributed-cap.md) — 分布式实现
- [NoelleNet.Ddd.Domain](ddd-domain.md) — 领域事件
