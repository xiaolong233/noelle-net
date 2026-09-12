# NoelleNet.Ddd.Domain

DDD 领域模型：实体、聚合根、值对象、领域事件、审计实体基类、仓储标记接口。**纯模型层，不含任何持久化实现** ✧

```bash
dotnet add package NoelleNet.Ddd.Domain
```

命名空间：`NoelleNet.Ddd.Domain.Entities`、`.Events`、`.Repositories`。依赖 [NoelleNet.Auditing](auditing.md)、[NoelleNet.Core](core.md)、[NoelleNet.EventBus](event-bus.md)。

⚠️ 本包的仓储只有接口（`IRepository<T>` 是空标记），实现见 [NoelleNet.EntityFrameworkCore](entity-framework-core.md)。

---

## 实体

### IEntity

```csharp
public interface IEntity { object?[] GetIdentifiers(); }
public interface IEntity<TIdentifier> : IEntity { TIdentifier Id { get; } }
```

### Entity / Entity<TIdentifier>

```csharp
public abstract class Entity : IEntity
{
    public abstract object?[] GetIdentifiers();
    public virtual bool IsTransient();
}

public abstract class Entity<TIdentifier> : Entity, IEntity<TIdentifier>
{
    protected Entity();
    protected Entity(TIdentifier id);
    public virtual TIdentifier Id { get; protected set; } = default!;
    public override object?[] GetIdentifiers() => [Id];
}
```

`IsTransient()` 判断「还没落库」：标识符为空数组，或任一位是 `null`、`Guid.Empty`、`0`（int / long）、空白字符串时为 `true`。

**相等性**按标识符而不是引用比较——这是实体与 DTO 最容易混淆的地方：

| 情形 | 结果 |
|------|------|
| 同一个引用 | 相等 |
| `GetType()` 不同 | 不相等 |
| **两个都是瞬态** | **仅引用相同才相等** |
| 一个瞬态、一个非瞬态 | 不相等 |
| 都非瞬态 | 逐位比较标识符 |

`==` / `!=` 运算符已重载，可直接用。⚠️ **没有实现 `IEquatable<T>`**，放进泛型集合时注意。

`ToString()` 输出形如 `TodoItem [Id: xxx]`、多标识符时 `[Ids: a, b]`、无标识符时 `[No Identifiers]`。

### ValueObject

```csharp
public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();
}
```

值对象按**所有成员**比较：`GetEqualityComponents()` 返回的序列逐项 `SequenceEqual` 判等，`GetHashCode()` 由同样的序列聚合。派生类只需把参与比较的字段列出来：

```csharp
public class Address : ValueObject
{
    public string Province { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Street { get; init; } = string.Empty;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Province;
        yield return City;
        yield return Street;
    }
}
```

`ToString()` 用 `JsonSerializer` 按**运行时类型**序列化（若按编译期类型 `ValueObject` 序列化会恒为 `{}`），输出非转义中文。

---

## 聚合根与领域事件

```csharp
public abstract class AggregateRoot<TIdentifier> : Entity<TIdentifier>, IAggregateRoot, IHasDomainEvents
{
    protected AggregateRoot();
    protected AggregateRoot(TIdentifier id);

    public IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    protected void AddDomainEvent(IDomainEvent eventData);
    protected void RemoveDomainEvent(IDomainEvent eventData);
    public void ClearDomainEvents();
}
```

⚠️ **`AddDomainEvent` 与 `RemoveDomainEvent` 是 `protected`**——只能在实体内部挂事件，外部（应用服务、仓储）改不了。`DomainEvents` 与 `ClearDomainEvents()` 才是 public，后者供拦截器在派发后清空。

```csharp
public class TodoItem : AuditedAggregateRoot<Guid>
{
    public void Complete()
    {
        IsCompleted = true;
        AddDomainEvent(new EntityUpdatedEvent<TodoItem>(this));
    }
}
```

### 事件类型

| 类型 | 定义 |
|------|------|
| `IDomainEvent` | 空标记接口 |
| `EntityCreatedEvent<TEntity>` | `record (TEntity Entity) where TEntity : IEntity` |
| `EntityUpdatedEvent<TEntity>` | 同上 |
| `EntityDeletedEvent<TEntity>` | 同上 |
| `EntityChangedEvent<TEntity>` | `record (TEntity Entity, EntityChangeType ChangeType)` |
| `EntityChangeType` | 枚举：`Create = 0`、`Update = 1`、`Delete = 2` |

`EntityCreated/Updated/DeletedEvent` 与 `EntityChangedEvent` 是**两套并存的模型**，选哪套由你决定——框架自身不产生任何领域事件，都得手动 `AddDomainEvent`。

### 处理器

```csharp
public interface IDomainEventHandler<in TEvent> : ILocalEventHandler<TEvent> where TEvent : IDomainEvent
{
}
```

它继承 [NoelleNet.EventBus](event-bus.md) 的 `ILocalEventHandler<TEvent>`，所以：

- 处理器用 `ILocalEventHandler<T>` 或 `IDomainEventHandler<T>` 都能被 `AddLocalEventBus` 扫描到；
- 领域事件最终通过 `ILocalEventBus` 派发，也就是说 **领域事件是本进程内的事件**，跨进程要另发集成事件。

```csharp
public class TodoItemCompletedHandler : IDomainEventHandler<EntityUpdatedEvent<TodoItem>>
{
    public Task HandleAsync(EntityUpdatedEvent<TodoItem> eventData, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
```

### 派发时机

由 `NoelleDomainEventInterceptor` 在 `SaveChanges` **提交前**派发，处理器在数据库事务内执行，详见 [NoelleNet.EntityFrameworkCore](entity-framework-core.md)。契约：

1. ✅ 处理器可以增删改实体，改动随本次保存一起提交；
2. ❌ 处理器内禁止再调 `SaveChanges`（嵌套保存会重复提交、状态不一致）；
3. 📤 短信、HTTP 调用等外部副作用请通过 `IDistributedEventBus` 发布集成事件；
4. 🚀 推荐用异步 `SaveChangesAsync`（同步路径会阻塞等待）。

---

## 审计实体基类

8 个基类，按「实体 / 聚合根」×「有标识符泛型 / 无泛型」×「仅创建审计 / 完整审计」组合：

| 类型 | 基类 | 实现的接口 | 审计属性 |
|------|------|-----------|----------|
| `CreationAuditedEntity` | `Entity` | `ICreationAudited` | `CreatedAt`、`CreatedBy` |
| `CreationAuditedEntity<TIdentifier>` | `Entity<TIdentifier>` | `ICreationAudited` | 同上 |
| `AuditedEntity` | `CreationAuditedEntity` | `IAudited` | 再加 `LastModifiedAt`、`LastModifiedBy` |
| `AuditedEntity<TIdentifier>` | `CreationAuditedEntity<TIdentifier>` | `IAudited` | 同上 |
| `CreationAuditedAggregateRoot` | `AggregateRoot` | `ICreationAudited` | `CreatedAt`、`CreatedBy` |
| `CreationAuditedAggregateRoot<TIdentifier>` | `AggregateRoot<TIdentifier>` | `ICreationAudited` | 同上 |
| `AuditedAggregateRoot` | `CreationAuditedAggregateRoot` | `IAudited` | 再加 `LastModifiedAt`、`LastModifiedBy` |
| `AuditedAggregateRoot<TIdentifier>` | `CreationAuditedAggregateRoot<TIdentifier>` | `IAudited` | 同上 |

聚合根版自带领域事件能力（继承自 `AggregateRoot`），实体版没有——**需要挂领域事件的只能是聚合根**。

几个细节：

- 审计属性都是 `virtual`，**只有 `AuditedAggregateRoot<TIdentifier>` 例外**（它的 `LastModifiedAt` / `LastModifiedBy` 没加 `virtual`），派生类想重写这两个属性时会撞上；
- `[MaxLength(64)]` 只加在 `CreatedBy` / `LastModifiedBy` 上，`CreatedAt` / `LastModifiedAt` 没有任何特性；
- 属性 setter 是 `protected`，填充由 [NoelleNet.EntityFrameworkCore](entity-framework-core.md) 的 `NoelleAuditInterceptor` 负责。

```csharp
public class TodoItem : AuditedAggregateRoot<Guid>   // 等价于「聚合根 + 完整审计 + Guid 主键」
{
    private TodoItem() { }                            // EF Core 专用

    public TodoItem(string title)
    {
        Id = Guid.CreateVersion7();
        Title = title;
        AddDomainEvent(new EntityCreatedEvent<TodoItem>(this));
    }

    public string Title { get; private set; } = string.Empty;
}
```

---

## 仓储

```csharp
public interface IRepository<T> where T : IAggregateRoot { }
```

**完全空的标记接口，没有任何成员**——框架不规定仓储该有哪些方法，按聚合的用例自己定：

```csharp
public interface ITodoItemRepository : IRepository<TodoItem>
{
    Task<TodoItem> AddAsync(TodoItem item, CancellationToken cancellationToken = default);
    Task<TodoItem?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Remove(TodoItem item);
}
```

实现见 [NoelleNet.EntityFrameworkCore](entity-framework-core.md) 的 `EfCoreRepository<TEntity, TDbContext>`。

---

## 相关包

- [NoelleNet.Auditing](auditing.md) — 审计接口
- [NoelleNet.EntityFrameworkCore](entity-framework-core.md) — 仓储实现、审计与领域事件拦截器
- [NoelleNet.EventBus](event-bus.md) — 领域事件最终经由本地事件总线派发
