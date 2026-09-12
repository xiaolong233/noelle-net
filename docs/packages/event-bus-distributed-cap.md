# NoelleNet.EventBus.Distributed.CAP

基于 [DotNetCore.CAP](https://cap.dotnetcore.xyz/) 的分布式（跨进程）事件总线实现 ✧

```bash
dotnet add package NoelleNet.EventBus
dotnet add package NoelleNet.EventBus.Distributed.CAP
```

关键命名空间：`NoelleNet.EventBus.Distributed`（`AddDistributedEventBus` 与 `UseCap`）和 `NoelleNet.EventBus.Distributed.CAP`（实现类型）。本包依赖 `DotNetCore.CAP` **10.0.2**——实现里继承了 CAP 的内部类型 `ConsumerServiceSelector` 并调用其 `protected` 成员，所以**与所编译的 CAP 版本强耦合**，升级 CAP 时要一并验证。

---

## 注册

`UseCap` 是 `DistributedEventBusConfiguration` 的扩展方法：

```csharp
services.AddDistributedEventBus(cfg =>
{
    cfg.RegisterServicesFromAssemblies(assembly);   // 扫描 IDistributedEventHandler<> 实现
    cfg.UseCap(options =>
    {
        options.UseEntityFramework<AppDbContext>();          // 事务发件箱的存储
        options.UseRabbitMQ(rb => { /* 主机、端口、账号等 */ });
        options.DefaultGroupName = configuration.GetRequiredValue("RabbitMQ:GroupName");
    });
});
```

`UseCap(Action<CapOptions>)` 做了三件事：

1. `AddScoped<IDistributedEventBus, CapDistributedEventBus>()` —— 注册总线本体；
2. `AddSingleton<IConsumerServiceSelector, NoelleConsumerServiceSelector>()` —— 换掉 CAP 默认的消费者选择器；
3. `AddCap(setupAction)` —— 注册 CAP 本体。

`CapOptions` 是 CAP 自己的配置类，用法与直接用 CAP 一致，详见其官方文档。

---

## 事件契约

事件类型用 `[EventName]`（来自 [NoelleNet.EventBus](event-bus.md)）声明事件名与分组：

```csharp
[EventName("todo.created", Group = "todo-service")]
public record TodoCreatedEvent(Guid TodoId, string Title);

public class TodoCreatedEventHandler : IDistributedEventHandler<TodoCreatedEvent>
{
    public Task HandleAsync(TodoCreatedEvent eventData, CancellationToken cancellationToken = default)
    {
        // 处理跨进程事件
        return Task.CompletedTask;
    }
}
```

发布：

```csharp
await distributedEventBus.PublishAsync(new TodoCreatedEvent(item.Id, item.Title));
await distributedEventBus.PublishDelayAsync(TimeSpan.FromMinutes(30), new OrderClosedEvent(orderId));
```

⚠️ **事件类型上没有 `[EventName]` 会在发布时直接抛 `InvalidOperationException("Event name cannot be empty")`**，`Name` 为空字符串同样如此。

⚠️ `PublishDelayAsync` 依赖 CAP 的持久化存储与调度器：消息先写入存储并带上到期时间，由调度器到期派发。用 InMemory 存储时**无法保证延迟生效**，延迟发布请配 SQL Server / MySQL / PostgreSQL 存储。

---

## 消费者是怎么挂上去的

CAP 默认只认识 `[CapSubscribe]` 标注的方法。本包的做法是：`AddDistributedEventBus` 扫描出的「处理器 ↔ 事件类型」配对会写进 `NoelleDistributedEventBusOptions.HandlerEventTypePairs`，再由 `NoelleConsumerServiceSelector`（继承 CAP 的 `ConsumerServiceSelector`）把这批配对翻译成 CAP 的消费者描述符。

翻译规则（`GetHandlerDescription`）：

1. 取事件类型上的 `[EventName]` 作为 topic，`Group` 作为分组，合成一个 `CapSubscribeAttribute`；
2. 若处理器的 `HandleAsync` 方法上**自己标了** `TopicAttribute`（继承自 CAP），则用你的，不再自动合成；
3. 参数标记：`[FromCap]` 标注的参数、以及 `CancellationToken` 类型的参数会被标记为 `IsFromCap`。

这带来两个可以直接利用的性质：

- 想给某个事件指定自定义 topic，在处理器的 `HandleAsync` 上标 `[CapSubscribe("custom.topic")]` 即可覆盖；
- 选择器是**叠加**在 CAP 原有发现结果之上的（先取 `base`，再追加本框架的描述符），所以同一个进程里本框架与原生 `[CapSubscribe]` 用法可以共存。

⚠️ **同一事件类型注册多个处理器时，只有第一个被扫描到的生效**——`AddDistributedEventBus` 用的是 `TryAddTransient`，其余会被静默忽略。需要「一个事件多个消费者」请用消息中间件的分组订阅（同一 topic 的多个 `Group` 各自消费），详见 [NoelleNet.EventBus](event-bus.md) 的扫描规则。

```csharp
public class TodoCacheInvalidationHandler : IDistributedEventHandler<TodoCreatedEvent>
{
    [CapSubscribe("todo.created.custom", Group = "cache-service")]
    public Task HandleAsync(TodoCreatedEvent eventData, CancellationToken cancellationToken = default)
        => _cache.InvalidateAsync(cancellationToken);
}
```

---

## 消息与数据的一致性

要让「发消息」和「写数据库」原子提交，需把 `ITransactionManager` 换成 CAP 事务发件箱实现（消息先落库，与业务数据同一事务提交）：

```csharp
services.Replace(ServiceDescriptor.Scoped<ITransactionManager, NoelleCapTransactionManager>());
```

对应实现包见 [NoelleNet.Extensions.CAP.SqlServer](extensions-cap-sqlserver.md)（另有 [MySql](extensions-cap-mysql.md) / [PostgreSql](extensions-cap-postgresql.md)）。事务发件箱的表结构由 CAP 自己维护，记得执行 CAP 的建表脚本或在启动时调用其初始化。

---

## 内部实现

| 类型 | 作用 |
|------|------|
| `CapDistributedEventBus` | `IDistributedEventBus` 实现，构造依赖 CAP 的 `ICapPublisher`；事件名从事件类型的 `[EventName]` 读取（`GetEventName` 是 `protected virtual`，可派生重写） |
| `NoelleConsumerServiceSelector` | 继承 CAP `ConsumerServiceSelector`，把扫描结果翻译为消费者描述符 |

---

## 相关包

- [NoelleNet.EventBus](event-bus.md) — 抽象层、`EventNameAttribute` 与扫描规则
- [NoelleNet.Extensions.CAP.SqlServer](extensions-cap-sqlserver.md) — 事务发件箱
- [NoelleNet.EntityFrameworkCore](entity-framework-core.md) — 发件箱的 EF Core 存储通常就用业务库
- [NoelleNet.EventBus.Local.MediatR](event-bus-local-mediatr.md) — 进程内事件
