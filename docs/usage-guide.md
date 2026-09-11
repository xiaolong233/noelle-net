# Noelle.Net 使用指南

本文是 README 的补充，按主题介绍各功能模块的详细用法。

---

## 全局异常处理

### 启用（.NET 8+ 推荐方式）

```csharp
builder.Services.AddLocalization();
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
        context.ProblemDetails.Instance = context.HttpContext.Request.Path;
});
builder.Services.AddExceptionHandler<NoelleExceptionHandler>();
builder.Services.AddSingleton<IErrorResponseWriter, ProblemDetailsErrorResponseWriter>();

// 调试信息仅在开发环境返回给客户端
builder.Services.Configure<NoelleExceptionHandlingOptions>(options =>
{
    options.IncludeExceptionDetails = env.IsDevelopment();
    options.IncludeExceptionData = env.IsDevelopment();
    options.IncludeStackTrace = env.IsDevelopment();
});

// 中间件
app.UseExceptionHandler();
```

也可以改用 MVC 异常过滤器 `NoelleExceptionHandlingFilter`（仅覆盖 MVC 管道内的异常）。

### 框架异常与状态码映射

| 异常 | HTTP 状态码 |
|------|-------------|
| `BusinessException` | 400 |
| `EntityNotFoundException` | 404 |
| `NoelleValidationException` | 400 |
| 数据库并发冲突（`DBConcurrencyException`） | 409 |
| `NotImplementedException` | 501 |
| 其他未处理异常 | 500 |

### 错误码与本地化

错误码集中在常量类中管理，领域层只抛错误码：

```csharp
// Domain.Shared/DomainErrorCodes.cs
public static class DomainErrorCodes
{
    public const string TodoItemTitleNull = "A01108";
    public const string OrganizationUnitParentNotFound = "A01124";
}

// 抛异常（占位参数通过 WithData 传入）
throw new BusinessException(DomainErrorCodes.OrganizationUnitParentNotFound)
    .WithData(nameof(parentId), parentId);
```

在 `AppResource.resx` 中定义错误码对应的本地化文本：

| Key | Value |
|-----|-------|
| `A01124` | 未找到标识符为 {parentId} 的父组织单元。 |

配置本地化资源来源：

```csharp
services.Configure<NoelleExceptionLocalizationOptions>(config =>
{
    // 第一个参数是当前异常，可用于按异常类型选择不同资源
    config.LocalizerProvider = (exception, factory) => factory.Create(typeof(AppResource));
});
```

框架根据 `ErrorCode` 查找本地化文本并替换 `{key}` 占位符；内置资源支持 `en` 与 `zh-CN`，可通过 `RequestLocalizationOptions` 配置支持的文化。

---

## 模型验证

框架提供两个验证过滤器，**底层验证机制不同，按项目实际情况选用其一即可**：

| 过滤器 | 底层机制 | 适用场景 |
|--------|----------|----------|
| `NoelleModelValidationFilter` | ASP.NET Core 内置的 `ModelState`（DataAnnotations 绑定期验证） | 使用 DataAnnotations 特性验证 |
| `NoelleFluentValidationFilter` | FluentValidation（按参数类型解析 `IValidator<T>`） | 使用 FluentValidation 编写验证规则 |

```csharp
builder.Services.AddControllers(options =>
{
    // 二选一
    options.Filters.Add<NoelleFluentValidationFilter>();
    // options.Filters.Add<NoelleModelValidationFilter>();
})
.ConfigureApiBehaviorOptions(options =>
{
    // 关闭 [ApiController] 内置的 ModelStateInvalidFilter 短路，让验证错误统一走框架异常处理。
    // 使用 NoelleModelValidationFilter 时必须设置（否则过滤器永远不会执行）；
    // 仅使用 NoelleFluentValidationFilter 时也建议设置（否则绑定期错误仍会输出内置格式）。
    options.SuppressModelStateInvalidFilter = true;
});
```

验证器注册与编写（验证器支持注入 `IStringLocalizer<T>` 本地化错误消息）：

```csharp
services.AddValidatorsFromAssembly(typeof(DependencyInjectionExtensions).Assembly);

public class CreateTodoItemDtoValidator : AbstractValidator<CreateTodoItemDto>
{
    public CreateTodoItemDtoValidator(IStringLocalizer<AppResource> localizer)
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(localizer["TodoItem.Title.NullErrorMessage"])
            .MaximumLength(TodoItemConstants.Title.MaxLength)
            .WithMessage(localizer["TodoItem.Title.MaxLengthErrorMessage", TodoItemConstants.Title.MaxLength]);
    }
}
```

验证失败响应（统一 ProblemDetails 格式）：

```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1",
  "title": "发生一个或多个验证错误。",
  "status": 400,
  "errors": { "Title": ["标题不能为空"] },
  "traceId": "..."
}
```

---

## 领域事件

领域事件由 `NoelleDomainEventInterceptor` 在 **`SaveChanges` 提交前**派发，处理器在数据库事务内执行，契约如下：

1. ✅ 处理器可以新增、修改或删除实体，改动会被本次 `SaveChanges` 捕获并随事务一起提交；
2. ❌ 处理器内禁止再次调用 `SaveChanges`（同一 DbContext 嵌套保存会导致重复提交与状态不一致）；
3. 📤 短信、HTTP 调用、缓存等外部副作用请通过 `IDistributedEventBus` 发布集成事件（配合 CAP 事务发件箱保证一致性），不要在处理器内直接执行；
4. 🚀 推荐使用异步 `SaveChangesAsync`（同步路径会阻塞等待事件处理完成）。

### 实体侧：挂载事件

```csharp
public static OrganizationUnit Create(string name, Guid? parentId, ...)
{
    // 校验...
    var unit = new OrganizationUnit { Id = Guid.CreateVersion7(), ... };
    unit.AddDomainEvent(new EntityChangedEvent<OrganizationUnit>(unit, EntityChangeType.Create));
    return unit;
}

// 更新事件注意去重：同一保存周期内多次修改只派发一次
private bool _updateEventAdded;

private void NotifyChanged()
{
    if (_updateEventAdded) return;
    _updateEventAdded = true;
    AddDomainEvent(new EntityChangedEvent<OrganizationUnit>(this, EntityChangeType.Update));
}
```

### 处理器侧：自动发现

```csharp
public class OrganizationUnitCacheInvalidationHandler : ILocalEventHandler<EntityChangedEvent<OrganizationUnit>>
{
    public Task HandleAsync(EntityChangedEvent<OrganizationUnit> eventData, CancellationToken cancellationToken = default)
        => _cache.InvalidateAsync(cancellationToken);
}
```

实现 `ILocalEventHandler<TEvent>`（领域事件也可用 `IDomainEventHandler<TEvent>`）即被 `AddLocalEventBus` 自动发现，无需手动注册。

---

## 审计

实体继承 `AuditedAggregateRoot<TIdentifier>`（或 `AuditedEntity<TIdentifier>`），注册 `NoelleAuditInterceptor` 后自动填充：

- 新增时：`CreatedAt`、`CreatedBy`（来自 `ICurrentUser.UserId`）
- 修改时：`LastModifiedAt`、`LastModifiedBy`

推荐的实体配置：审计列统一 snake_case 命名；审计基类（`AuditedAggregateRoot` / `AuditedEntity` 等）已内置 `[MaxLength(64)]`，无需重复声明长度：

```csharp
public static void ConfigureAuditingProperties<TEntity>(this EntityTypeBuilder<TEntity> builder) where TEntity : class, IAudited
{
    builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasComment("创建时间");
    builder.Property(x => x.CreatedBy).HasColumnName("created_by").HasComment("创建人");
    builder.Property(x => x.LastModifiedAt).HasColumnName("last_modified_at").HasComment("最后修改时间");
    builder.Property(x => x.LastModifiedBy).HasColumnName("last_modified_by").HasComment("最后修改人");
}
```

---

## 工作单元与事务

`NoelleTransactionBehavior`（MediatR 行为管道）为每个命令自动开启事务：`BeginAsync → 处理命令 → SaveChangesAsync → CommitAsync`，异常时回滚。因此**命令处理器不需要调用 SaveChanges**。

需要消息与数据库操作原子提交时，使用 CAP 事务发件箱——把事务管理器替换为对应数据库的实现：

```csharp
services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddScoped<ITransactionManager, NoelleTransactionManager>();      // 默认（仅 EF 事务）
// 支持 CAP 事务发件箱时替换（需安装 NoelleNet.Extensions.CAP.*）：
services.Replace(ServiceDescriptor.Scoped<ITransactionManager, NoelleCapTransactionManager>());
```

> 注意：`ITransactionManager` 为 Scoped 服务，同一请求内命令管道共用同一事务；`HasActiveTransaction` 保证嵌套命令不会重复开启事务。

---

## 事件总线

### 本地事件（进程内，MediatR）

```csharp
services.AddLocalEventBus(cfg =>
{
    cfg.RegisterServicesFromAssemblies(assembly);
    cfg.UseMediatR(x =>
    {
        x.RegisterServicesFromAssemblies(assembly);
        x.AddOpenBehavior(typeof(NoelleLoggingBehavior<,>));
        x.AddOpenBehavior(typeof(NoelleTransactionBehavior<,>));
    });
});
```

`ILocalEventHandler<TEvent>` 的实现会被自动发现并注册，事件通过 `ILocalEventBus.PublishAsync` 派发。

### 分布式事件（跨进程，CAP）

```csharp
[EventName("todo.created", Group = "todo-service")]
public record TodoCreatedEvent(Guid TodoId, string Title);

// 发布
await _distributedEventBus.PublishAsync(new TodoCreatedEvent(id, title));
await _distributedEventBus.PublishDelayAsync(TimeSpan.FromMinutes(30), orderClosed);   // 延迟发布
```

```csharp
services.AddDistributedEventBus(cfg =>
{
    cfg.RegisterServicesFromAssemblies(assembly);
    cfg.UseCap(options =>
    {
        options.UseEntityFramework<AppDbContext>();
        options.UseRabbitMQ(rb => { /* ... */ });
        options.DefaultGroupName = configuration.GetRequiredValue("RabbitMQ:GroupName");
    });
});
```

实现 `IDistributedEventHandler<TEvent>` 即被自动发现。

注意：

- 事件名与分组通过 `EventNameAttribute` 声明；分布式事件总线依赖 CAP 的消费者选择器扩展点，CAP 版本要求不低于 `8.4.1`；
- `PublishDelayAsync` 依赖 CAP 的持久化存储与调度器，InMemory 存储下无法保证延迟生效；
- 消息与业务数据原子提交：使用 CAP 事务发件箱（见"工作单元与事务"）。

---

## 安全主体

`ICurrentUser` 声明解析遵循"OpenID Connect 短名优先、`ClaimTypes` URI 类型回退"策略，兼容 Cookie、JWT Bearer、OpenIddict 等认证方案：

```csharp
services.AddHttpContextAccessor();
services.AddScoped<ICurrentUser, CurrentUser>();
services.AddScoped<ICurrentPrincipalProvider, NoelleHttpContextCurrentPrincipalProvider>();
```

```csharp
// 应用层：数据归属校验（不向非超管暴露他人数据的存在性）
if (!_currentUser.IsInRole(PermissionConstants.SuperAdminRole) && item.CreatedBy != _currentUser.UserId)
    throw new EntityNotFoundException<TodoItem>(id);
```

---

## 应用 DTO

`NoelleNet.Application.Contracts` 提供分页、排序、列表结果等通用对象：

```csharp
public class TodoItemPagingDto : PagingDto
{
    public string? Title { get; set; }
    public bool? IsCompleted { get; set; }
}

// 查询实现
var query = _dbContext.TodoItems
    .AsNoTracking()
    .WhereIf(!string.IsNullOrWhiteSpace(dto.Title), x => x.Title.Contains(dto.Title!))
    .WhereIf(dto.IsCompleted.HasValue, x => x.IsCompleted == dto.IsCompleted!.Value);

var totalCount = await query.CountAsync(cancellationToken);

// Sort 是约定格式字符串（如 "CreatedAt desc"、"CreatedAt desc, Title asc"），框架不内置字符串排序，
// 需自行解析映射为排序表达式（或引入 System.Linq.Dynamic.Core 按字符串排序）
var items = await query
    .OrderByDescending(x => x.CreatedAt)
    .Skip(dto.Offset).Take(dto.Limit)
    .ToListAsync(cancellationToken);

return new PagedResultDto<TodoItemDto>(totalCount, items);
```

分页模型为 offset-based（`Offset` 跳过 N 条 + `Limit` 取 M 条，对应 SQL `LIMIT/OFFSET` 与 OData `$skip/$top` 语义）。`Limit` 与 `Offset` 自带钳制（负值按 0/默认值处理，超上限截断）。

**设计规则**：DTO 属性默认非 `virtual`；仅当属性内嵌默认钳制行为、且该行为需要按场景定制时声明 `virtual`（当前有 `LimitDto.Limit`、`PagingDto.Offset`），派生类可重写实现按端点的自定义上限或严格校验（如抛异常）。

---

## 其他基础能力（NoelleNet.Core）

- `IGuidGenerator`：默认实现生成有序的 `Guid.CreateVersion7()`，利于数据库索引；
- `IDistributedCache` 泛型扩展：`GetAsync<T>` / `SetAsync<T>` / `GetOrCreateAsync<T>`；
- `IConfiguration` 强校验取值：`GetRequiredValue` / `GetRequiredConnectionString`（缺失时抛出含配置路径的异常）；
- `TokenManagerBase`：带过期缓冲与双重检查锁的访问令牌管理基类；
- 扩展方法：`WhereIf`（Enumerable/Queryable）、`WithData`（异常附加数据）、`To<T>`（类型转换）、`GetGenericTypeName`（泛型类型名）、`IsNullOrEmpty`/`IsNullOrWhiteSpace`（空值判断）等。
