# Noelle.Net

Noelle.Net 是一个面向 **.NET 9** WebApi 开发的应用基础类库，提供 DDD（领域驱动设计）实体模型、事件总线、EF Core 仓储、审计追踪、工作单元、ASP.NET Core 增强等常用组件。

设计原则：不深度封装 ASP.NET Core、依赖注入显式配置（无模块化与自动注册）、优先使用社区成熟组件（MediatR / CAP / FluentValidation / EF Core）、暂不支持多租户。

---

## 安装

所有包均通过 [NuGet](https://www.nuget.org/) 分发，包名前缀统一为 `NoelleNet.*`。根据项目需要选择安装：

```bash
# 核心基础库
dotnet add package NoelleNet.Core

# DDD 领域模型 + 审计
dotnet add package NoelleNet.Ddd.Domain

# EF Core 仓储 & 工作单元 & 拦截器
dotnet add package NoelleNet.EntityFrameworkCore

# 事件总线（本地 + 分布式）
dotnet add package NoelleNet.EventBus
dotnet add package NoelleNet.EventBus.Local.MediatR
dotnet add package NoelleNet.EventBus.Distributed.CAP

# CAP 数据库提供程序（事务发件箱）
dotnet add package NoelleNet.Extensions.CAP.SqlServer
dotnet add package NoelleNet.Extensions.CAP.MySql
dotnet add package NoelleNet.Extensions.CAP.PostgreSql

# MediatR 管道行为扩展（日志、事务管理）
dotnet add package NoelleNet.Extensions.MediatR

# ASP.NET Core 增强
dotnet add package NoelleNet.AspNetCore

# 应用层 DTO
dotnet add package NoelleNet.Application.Contracts
```

---

## 包总览

| 包名 | 描述 |
|------|------|
| **NoelleNet.Core** | 基础库：异常、安全、序列化、缓存、工具类 |
| **NoelleNet.Auditing** | 审计接口：`ICreationAudited`、`IModificationAudited`、`IAudited` |
| **NoelleNet.Uow** | 工作单元与事务管理器接口：`IUnitOfWork`、`ITransactionManager` |
| **NoelleNet.Ddd.Domain** | DDD 领域模型：`Entity`、`AggregateRoot`、`ValueObject`、领域事件、审计实体基类、`IRepository` |
| **NoelleNet.EventBus** | 事件总线抽象：`ILocalEventBus`、`IDistributedEventBus`、`EventNameAttribute` |
| **NoelleNet.EventBus.Local.MediatR** | 基于 [MediatR](https://github.com/jbogard/MediatR) 的本地事件总线实现 |
| **NoelleNet.EventBus.Distributed.CAP** | 基于 [DotNetCore.CAP](https://cap.dotnetcore.xyz/) 的分布式事件总线实现 |
| **NoelleNet.EntityFrameworkCore** | EF Core 集成：`EfCoreRepository`、`UnitOfWork`、审计拦截器、领域事件拦截器、自动 GUID 键拦截器 |
| **NoelleNet.AspNetCore** | ASP.NET Core 增强：全局异常处理（ProblemDetails）、模型验证（DataAnnotations + FluentValidation）、路由 kebab-case 转换 |
| **NoelleNet.Application.Contracts** | 应用层 DTO：分页/排序/列表结果、审计 DTO 基类 |
| **NoelleNet.Extensions.MediatR** | MediatR 管道行为：日志记录、事务自动管理 |
| **NoelleNet.Extensions.CAP.SqlServer** | CAP + SQL Server 事务管理器 |
| **NoelleNet.Extensions.CAP.MySql** | CAP + MySQL 事务管理器 |
| **NoelleNet.Extensions.CAP.PostgreSql** | CAP + PostgreSQL 事务管理器 |

---

## 快速入门

更详细的模块用法见 GitHub 仓库的 [docs/usage-guide.md](https://github.com/xiaolong233/noelle-net/blob/master/docs/usage-guide.md)。

### 1. 定义领域模型

```csharp
public class TodoItem : AuditedAggregateRoot<Guid>
{
    private TodoItem() { }   // EF Core 专用

    public TodoItem(string title)
    {
        Id = Guid.NewGuid();
        Title = title;
        AddDomainEvent(new EntityCreatedEvent<TodoItem>(this));
    }

    public string Title { get; private set; } = string.Empty;
    public bool IsCompleted { get; private set; }

    public void Complete()
    {
        IsCompleted = true;
        AddDomainEvent(new EntityUpdatedEvent<TodoItem>(this));
    }
}
```

### 2. 定义仓储

```csharp
// 接口
public interface ITodoItemRepository : IRepository<TodoItem>
{
    Task<TodoItem> AddAsync(TodoItem item, CancellationToken cancellationToken = default);
    Task<TodoItem?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Remove(TodoItem item);
}

// 实现
public class TodoItemRepository : EfCoreRepository<TodoItem, AppDbContext>, ITodoItemRepository
{
    public TodoItemRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    /// <inheritdoc/>
    public async Task<TodoItem> AddAsync(TodoItem item, CancellationToken cancellationToken = default)
    {
        var entry = await DbContext.TodoItems.AddAsync(item, cancellationToken);
        return entry.Entity;
    }

    /// <inheritdoc/>
    public Task<TodoItem?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => DbContext.TodoItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    /// <inheritdoc/>
    public void Remove(TodoItem item) => DbContext.TodoItems.Remove(item);
}
```

### 3. 注册服务（显式配置）

```csharp
// 安全主体
services.AddHttpContextAccessor();
services.AddSingleton<ICurrentPrincipalProvider, NoelleHttpContextCurrentPrincipalProvider>();
services.AddScoped<ICurrentUser, CurrentUser>();

// GUID 生成器
services.AddSingleton<IGuidGenerator, NoelleGuidGenerator>();

// 全局异常处理（.NET 8+ 推荐方式）
services.AddLocalization();
services.AddProblemDetails();
services.AddSingleton<IErrorResponseWriter, ProblemDetailsErrorResponseWriter>();
services.AddExceptionHandler<NoelleExceptionHandler>();

// 模型验证（成对使用，见"模型验证"一节）
services.AddControllers(options =>
{
    options.Filters.Add<NoelleModelValidationFilter>();
    options.Filters.Add<NoelleFluentValidationFilter>();
});
services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

// 数据库 + 拦截器
// AddDbContext<DbContext, AppDbContext> 会同时注册两个服务类型：
// 框架组件（UnitOfWork 等）依赖抽象的 DbContext，仓储依赖具体的 AppDbContext
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

// 工作单元
services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddScoped<ITransactionManager, NoelleTransactionManager>();

// 事件总线
var assemblies = new[] { typeof(Program).Assembly };
services.AddLocalEventBus(cfg =>
{
    cfg.RegisterServicesFromAssemblies(assemblies);
    cfg.UseMediatR(x => x.RegisterServicesFromAssemblies(assemblies));
});

// 仓储（显式注册）
services.AddScoped<ITodoItemRepository, TodoItemRepository>();
```

### 4. 使用

```csharp
public class TodoAppService(ITodoItemRepository repository, IUnitOfWork unitOfWork)
{
    public async Task<TodoItem> CreateAsync(string title)
    {
        var item = new TodoItem(title);
        await repository.AddAsync(item);
        await unitOfWork.SaveChangesAsync();
        return item;
    }
}
```

---

## 异常处理

```csharp
// 业务异常（HTTP 400）
throw new BusinessException(errorCode: "TODO:DUPLICATE_NAME", message: "已存在同名的待办事项");

// 实体未找到（HTTP 404）
throw new EntityNotFoundException(typeof(TodoItem), itemId);
```

异常自动转换为 RFC 9457 ProblemDetails：

```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.5",
  "title": "请求的资源未找到。",
  "status": 404,
  "detail": "未找到标识符为 1 的 TodoItem 实体。",
  "code": "TODO:NOT_FOUND",
  "traceId": "..."
}
```

**错误码本地化**：

```csharp
builder.Services.Configure<NoelleExceptionLocalizationOptions>(options =>
{
    options.LocalizerProvider = (exception, factory) =>
        factory.Create(typeof(NoelleExceptionHandlingResource));
});
```

抛异常时通过 `Data` 传递占位参数，框架按 `ErrorCode` 查找本地化文本并替换 `{key}` 占位符：

```csharp
var ex = new BusinessException(errorCode: "TODO:DUPLICATE_NAME");
ex.Data["title"] = "买菜";
throw ex;
```

---

## 模型验证

两个验证过滤器分工：

| 过滤器 | 职责 |
|---|---|
| `NoelleModelValidationFilter` | 绑定期验证（DataAnnotations、类型转换失败） |
| `NoelleFluentValidationFilter` | 业务规则验证（按参数类型解析 `IValidator<T>`） |

⚠️ `NoelleModelValidationFilter` 必须与 `SuppressModelStateInvalidFilter = true` **成对使用**：压制 ASP.NET Core 内置验证短路，验证错误统一走框架异常处理；只压制而不注册该过滤器，会导致无效模型直接进入 Action。

---

## 领域事件

领域事件在 `SaveChanges` **提交前** 派发，处理器在数据库事务内执行：

1. ✅ 处理器可以增删改实体，改动随本次保存一起提交；
2. ❌ 处理器内禁止嵌套调用 `SaveChanges`；
3. 📤 外部副作用请通过 `IDistributedEventBus` 发布集成事件（配合 CAP 事务发件箱）；
4. 🚀 推荐使用异步 `SaveChangesAsync`。

---

## 分布式事件与延迟发布

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

注意：`IDistributedEventBus.PublishDelayAsync` 的延迟发布依赖 CAP 的持久化存储与调度器，InMemory 存储下无法保证延迟生效。

---

## 适用范围

- **目标框架**: .NET 9
- **许可证**: MIT
- **仓库**: [github.com/xiaolong233/noelle-net](https://github.com/xiaolong233/noelle-net)
