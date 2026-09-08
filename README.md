# Noelle.Net

面向 **.NET 9** WebApi 开发的应用基础类库。它提供 DDD 领域模型、事件总线、EF Core 仓储、审计追踪、工作单元与 ASP.NET Core 增强等开箱即用的组件，帮助快速搭建风格统一的 WebApi 项目。

[![NuGet](https://img.shields.io/badge/nuget-v9.1.5-blue)](https://www.nuget.org/)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com/)

---

## 设计原则

- **不深度封装 ASP.NET Core** — 框架只做增强（异常处理、模型验证、路由等），不替代原生能力，也不会把你关进黑箱
- **依赖注入显式配置** — 不提供模块化自动装配；注册了什么、怎么注册，代码里一目了然
- **社区成熟组件优先** — 事件总线基于 MediatR / CAP，验证基于 FluentValidation，数据访问基于 EF Core
- **暂不支持多租户**

---

## 功能一览

| 能力 | 说明 | 所在包 |
|------|------|--------|
| DDD 领域模型 | `Entity` / `AggregateRoot` / `ValueObject` 基类、领域事件 | `NoelleNet.Ddd.Domain` |
| 审计追踪 | 创建时间/创建人/修改时间/修改人自动填充 | `NoelleNet.Auditing` + `NoelleNet.EntityFrameworkCore` |
| EF Core 仓储 | `EfCoreRepository<T>` 薄封装，内置审计/领域事件/GUID 拦截器 | `NoelleNet.EntityFrameworkCore` |
| 工作单元与事务 | `IUnitOfWork` / `ITransactionManager`，支持 CAP 事务发件箱 | `NoelleNet.Uow` + `NoelleNet.Extensions.CAP.*` |
| 事件总线 | 本地（MediatR）+ 分布式（CAP），统一抽象、按接口自动发现处理器 | `NoelleNet.EventBus.*` |
| 全局异常处理 | 异常自动转为 RFC 9457 ProblemDetails，支持错误码本地化 | `NoelleNet.AspNetCore` |
| 模型验证 | DataAnnotations 绑定验证 + FluentValidation 业务验证，统一错误格式 | `NoelleNet.AspNetCore` |
| 安全主体 | `ICurrentUser` 统一访问当前用户（OIDC 短名声明） | `NoelleNet.Core` |
| 应用 DTO | 分页 / 排序 / 列表结果等通用对象 | `NoelleNet.Application.Contracts` |
| MediatR 行为管道 | 命令日志、事务自动管理 | `NoelleNet.Extensions.MediatR` |

---

## 快速开始

```bash
dotnet add package NoelleNet.Ddd.Domain
dotnet add package NoelleNet.EntityFrameworkCore
dotnet add package NoelleNet.AspNetCore
dotnet add package NoelleNet.EventBus.Local.MediatR
```

领域实体（继承审计聚合根，自带 `Id` 与审计字段）：

```csharp
public class TodoItem : AuditedAggregateRoot<Guid>
{
    protected TodoItem() { }   // EF Core 专用

    public TodoItem(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new BusinessException("A01108", "待办事项标题不能为空");

        Id = Guid.CreateVersion7();
        Title = title;
    }

    public string Title { get; set; } = null!;
    public bool IsCompleted { get; set; }

    public void Complete() => IsCompleted = true;
}
```

服务注册骨架（各功能模块的注册入口，详见下文逐项说明）：

```csharp
// 安全主体
services.AddHttpContextAccessor();
services.AddScoped<ICurrentUser, CurrentUser>();
services.AddScoped<ICurrentPrincipalProvider, NoelleHttpContextCurrentPrincipalProvider>();

// 全局异常处理
services.AddLocalization();
services.AddProblemDetails();
services.AddExceptionHandler<NoelleExceptionHandler>();
services.AddSingleton<IErrorResponseWriter, ProblemDetailsErrorResponseWriter>();

// 控制器：模型验证过滤器 + kebab-case 路由
services.AddControllers(options =>
{
    options.Filters.Add<NoelleFluentValidationFilter>();
    options.Conventions.Add(new RouteTokenTransformerConvention(new NoelleRouteKebabCaseTransformer()));
}).ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true);
builder.Services.AddValidatorsFromAssemblyContaining<CreateTodoInputValidator>();

// 数据库 + 拦截器（AddDbContext<DbContext, AppDbContext> 会同时注册抽象与具体两个服务类型）
services.AddScoped<NoelleAutoSetGuidKeyInterceptor>();
services.AddScoped<NoelleAuditInterceptor>();
services.AddScoped<NoelleDomainEventInterceptor>();
services.AddDbContext<DbContext, AppDbContext>((sp, options) =>
{
    options.UseNpgsql(configuration.GetRequiredConnectionString("Default"));
    options.AddInterceptors(
        sp.GetRequiredService<NoelleAutoSetGuidKeyInterceptor>(),
        sp.GetRequiredService<NoelleAuditInterceptor>(),
        sp.GetRequiredService<NoelleDomainEventInterceptor>());
});

// 工作单元与事务
services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddScoped<ITransactionManager, NoelleTransactionManager>();

// 本地事件总线
var assembly = typeof(Program).Assembly;
services.AddLocalEventBus(cfg =>
{
    cfg.RegisterServicesFromAssemblies(assembly);
    cfg.UseMediatR(x => x.RegisterServicesFromAssemblies(assembly));
});
```

各功能模块的注册与用法细节见 [docs/usage-guide.md](docs/usage-guide.md)。

---

## 全局异常处理

启用后，异常自动转为符合 RFC 9457 的 ProblemDetails 响应：

```csharp
// 业务异常 → HTTP 400
throw new BusinessException("A01108", "待办事项标题不能为空");

// 实体未找到 → HTTP 404
throw new EntityNotFoundException(typeof(TodoItem), id);
```

| 异常 | HTTP 状态码 |
|------|-------------|
| `BusinessException` | 400 |
| `EntityNotFoundException` | 404 |
| `NoelleValidationException` | 400 |
| 数据库并发冲突 | 409 |
| 其他未处理异常 | 500 |

支持错误码本地化：通过 `NoelleExceptionLocalizationOptions.LocalizerProvider` 指定资源类型，`ErrorCode` 映射 resx 文本，`WithData` 传占位参数。详细说明见 [docs/usage-guide.md](docs/usage-guide.md#全局异常处理)。

---

## 模型验证

框架提供两个验证过滤器，**底层验证机制不同，按项目实际情况选用其一即可**：

| 过滤器 | 底层机制 | 适用场景 |
|--------|----------|----------|
| `NoelleModelValidationFilter` | ASP.NET Core 内置的 `ModelState`（DataAnnotations 绑定期验证） | 使用 DataAnnotations 特性验证 |
| `NoelleFluentValidationFilter` | FluentValidation（按参数类型解析 `IValidator<T>`） | 使用 FluentValidation 编写验证规则 |

两者都抛出 `NoelleValidationException`，输出统一错误格式：

```csharp
builder.Services.AddControllers(options =>
{
    // 二选一
    options.Filters.Add<NoelleFluentValidationFilter>();
    // options.Filters.Add<NoelleModelValidationFilter>();
})
.ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true);
```

> ⚠️ `SuppressModelStateInvalidFilter = true` 用于关闭 `[ApiController]` 内置验证短路，让验证错误统一走框架异常处理。使用 `NoelleModelValidationFilter` 时**必须**设置（否则过滤器永远不会执行）；仅使用 `NoelleFluentValidationFilter` 时也建议设置（否则绑定期错误仍会输出内置格式）。

验证器注册后自动生效，支持注入本地化器：

```csharp
builder.Services.AddValidatorsFromAssemblyContaining<CreateTodoInputValidator>();
```

---

## 领域事件

聚合根内挂载领域事件，由 `NoelleDomainEventInterceptor` 在 **`SaveChanges` 提交前**派发，处理器在数据库事务内执行：

```csharp
unit.AddDomainEvent(new EntityChangedEvent<OrganizationUnit>(unit, EntityChangeType.Create));
```

```csharp
public class OrganizationUnitCacheInvalidationHandler : ILocalEventHandler<EntityChangedEvent<OrganizationUnit>>
{
    public Task HandleAsync(EntityChangedEvent<OrganizationUnit> eventData, CancellationToken cancellationToken = default)
        => _cache.InvalidateAsync(cancellationToken);
}
```

契约：

1. ✅ 处理器可以增删改实体，改动会被本次 `SaveChanges` 捕获并随事务一起提交；
2. ❌ 处理器内禁止再次调用 `SaveChanges`（嵌套保存会导致重复提交与状态不一致）；
3. 📤 短信、HTTP 调用等外部副作用请通过分布式事件发布（配合 CAP 事务发件箱），不要在处理器内直接执行。

---

## 审计

实体继承 `AuditedAggregateRoot<TIdentifier>`（或 `AuditedEntity<TIdentifier>`），注册 `NoelleAuditInterceptor` 后自动填充：

- 新增时：`CreatedAt`、`CreatedBy`（来自 `ICurrentUser.UserId`）
- 修改时：`LastModifiedAt`、`LastModifiedBy`

---

## 工作单元与事务

```csharp
services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddScoped<ITransactionManager, NoelleTransactionManager>();
```

- `IUnitOfWork.SaveChangesAsync` 统一保存变更；
- 配合 MediatR 的 `NoelleTransactionBehavior`（`NoelleNet.Extensions.MediatR`），每个命令自动完成"开启事务 → 处理 → 保存 → 提交/回滚"，处理器内无需调用 `SaveChanges`；
- 需要消息与数据库操作原子提交时，把事务管理器替换为 CAP 事务发件箱实现：

```csharp
services.Replace(ServiceDescriptor.Scoped<ITransactionManager, NoelleCapTransactionManager>());
```

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
        // 可选：为 MediatR 命令添加行为管道
        x.AddOpenBehavior(typeof(NoelleLoggingBehavior<,>));
        x.AddOpenBehavior(typeof(NoelleTransactionBehavior<,>));
    });
});
```

实现 `ILocalEventHandler<TEvent>` 即被自动发现，通过 `ILocalEventBus.PublishAsync` 派发。

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
        options.UseRabbitMQ(rb => { /* 主机、端口、账号等 */ });
        options.DefaultGroupName = configuration.GetRequiredValue("RabbitMQ:GroupName");
    });
});
```

实现 `IDistributedEventHandler<TEvent>` 即被自动发现。

> 注意：`PublishDelayAsync` 依赖 CAP 的持久化存储与调度器，InMemory 存储下无法保证延迟生效。

---

## 安全主体

`ICurrentUser` 统一访问当前用户，声明解析遵循"OpenID Connect 短名优先、`ClaimTypes` URI 回退"策略，兼容 Cookie、JWT Bearer、OpenIddict 等认证方案：

```csharp
services.AddHttpContextAccessor();
services.AddScoped<ICurrentUser, CurrentUser>();
services.AddScoped<ICurrentPrincipalProvider, NoelleHttpContextCurrentPrincipalProvider>();
```

```csharp
public class TodoAppService(ICurrentUser currentUser)
{
    public bool CanManage(TodoItem item) =>
        currentUser.IsInRole("admin") || item.CreatedBy == currentUser.UserId;
}
```

---

## 应用 DTO

`NoelleNet.Application.Contracts` 提供分页、排序、列表结果等通用对象：

```csharp
// 请求：PagingDto（Offset / Limit / Sort，offset-based 分页模型）
// 结果：ListResultDto<T> / PagedResultDto<T>
return new PagedResultDto<TodoItemDto>(totalCount, items);
```

---

## 包列表

| 包名 | 说明 |
|------|------|
| `NoelleNet.Core` | 基础库：异常、安全、序列化、缓存、工具类 |
| `NoelleNet.Ddd.Domain` | DDD 领域模型：实体、聚合根、值对象、领域事件 |
| `NoelleNet.EntityFrameworkCore` | EF Core 仓储、工作单元、审计/领域事件拦截器 |
| `NoelleNet.EventBus` | 事件总线抽象：本地 + 分布式 |
| `NoelleNet.EventBus.Local.MediatR` | 基于 MediatR 的本地事件总线实现 |
| `NoelleNet.EventBus.Distributed.CAP` | 基于 CAP 的分布式事件总线实现 |
| `NoelleNet.AspNetCore` | 全局异常、验证、KebabCase 路由 |
| `NoelleNet.Application.Contracts` | 应用层 DTO：分页、排序、列表结果 |
| `NoelleNet.Extensions.MediatR` | MediatR 管道行为：日志、事务管理 |
| `NoelleNet.Auditing` | 审计接口定义 |
| `NoelleNet.Uow` | 工作单元与事务管理器接口 |
| `NoelleNet.Extensions.CAP.SqlServer` | CAP + SQL Server 事务管理器 |
| `NoelleNet.Extensions.CAP.MySql` | CAP + MySQL 事务管理器 |
| `NoelleNet.Extensions.CAP.PostgreSql` | CAP + PostgreSQL 事务管理器 |

---

## 贡献

欢迎通过 [GitHub Issues](https://github.com/xiaolong233/noelle-net/issues) 提交 Bug 或功能建议，也欢迎 Pull Request。

```powershell
# 构建
dotnet build framework/Noelle.Net.slnx

# 测试
dotnet test framework/Noelle.Net.slnx

# 打包（同时生成 .snupkg 符号包）
cd nupkg && ./pack.ps1

# 推送（可选：打包后自动推送主包与符号包，需先设置 NUGET_API_KEY）
cd nupkg; $env:NUGET_API_KEY = '<your key>'; ./pack.ps1 -Push
```

---

## 许可证

[MIT](LICENSE) © 2024 [xiaolong233](https://github.com/xiaolong233)
