# Noelle.Net

这是辅助 .NET 开发的小类库～ 平时顺手攒的小零件都在这儿了，希望能帮你少敲几行重复代码！(｡•̀ᴗ-)✧

---

## 安装

包名前缀统一为 `NoelleNet.*`，按需选择：

```bash
# 核心（异常、安全、序列化、缓存、工具类）
dotnet add package NoelleNet.Core

# DDD 领域模型 + 审计接口
dotnet add package NoelleNet.Ddd.Domain

# EF Core 仓储、工作单元、拦截器
dotnet add package NoelleNet.EntityFrameworkCore

# 事件总线：本地（MediatR）+ 分布式（CAP）
dotnet add package NoelleNet.EventBus.Local.MediatR
dotnet add package NoelleNet.EventBus.Distributed.CAP

# 消息与数据库原子提交时，再引入对应数据库的 CAP 事务管理器
dotnet add package NoelleNet.Extensions.CAP.SqlServer   # 或 .MySql / .PostgreSql

# ASP.NET Core 增强（异常处理、模型验证、路由）+ MediatR 管道
dotnet add package NoelleNet.AspNetCore
dotnet add package NoelleNet.Extensions.MediatR
```

---

## 包说明

| 包名 | 描述 |
|------|------|
| **NoelleNet.Core** | 基础库：异常、安全、序列化、缓存、工具类 |
| **NoelleNet.Auditing** | 审计接口：`ICreationAudited`、`IModificationAudited`、`IAudited` |
| **NoelleNet.Uow** | 工作单元与事务管理器接口：`IUnitOfWork`、`ITransactionManager` |
| **NoelleNet.Ddd.Domain** | DDD 领域模型：`Entity`、`AggregateRoot`、`ValueObject`、领域事件、审计实体基类、`IRepository` |
| **NoelleNet.EventBus** | 事件总线抽象：`ILocalEventBus`、`IDistributedEventBus`、`EventNameAttribute` |
| **NoelleNet.EventBus.Local.MediatR** | 基于 [MediatR](https://github.com/jbogard/MediatR) 的本地事件总线 |
| **NoelleNet.EventBus.Distributed.CAP** | 基于 [DotNetCore.CAP](https://cap.dotnetcore.xyz/) 的分布式事件总线 |
| **NoelleNet.EntityFrameworkCore** | EF Core 集成：`EfCoreRepository`、`UnitOfWork`、审计 / 领域事件 / 自动 GUID 键拦截器 |
| **NoelleNet.AspNetCore** | 全局异常处理（ProblemDetails）、模型验证（DataAnnotations + FluentValidation）、路由 kebab-case |
| **NoelleNet.Application.Contracts** | 应用层 DTO：分页 / 排序 / 列表结果、审计 DTO 基类 |
| **NoelleNet.Extensions.MediatR** | MediatR 管道行为：日志记录、事务自动管理 |
| **NoelleNet.Extensions.CAP.SqlServer** | CAP + SQL Server 事务管理器（另有 `.MySql` / `.PostgreSql`） |

---

## 快速入门

### 1. 定义领域模型

```csharp
public class TodoItem : AuditedAggregateRoot<Guid>
{
    private TodoItem() { }   // EF Core 专用

    public TodoItem(string title)
    {
        Id = Guid.CreateVersion7();
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

### 2. 注册服务

没有自动注册，以下为各模块的注册入口（`AddDbContext<DbContext, AppDbContext>` 会同时注册抽象与具体两个服务类型）：

```csharp
// 安全主体 + GUID 生成器
services.AddHttpContextAccessor();
services.AddScoped<ICurrentUser, CurrentUser>();
services.AddScoped<ICurrentPrincipalProvider, NoelleHttpContextCurrentPrincipalProvider>();
services.AddSingleton<IGuidGenerator, NoelleGuidGenerator>();

// 全局异常处理（管道中还需调用 app.UseExceptionHandler()）
services.AddLocalization();
services.AddProblemDetails();
services.AddSingleton<IErrorResponseWriter, ProblemDetailsErrorResponseWriter>();
services.AddExceptionHandler<NoelleExceptionHandler>();

// 模型验证：两个过滤器二选一，并需注册验证器
services.AddControllers(options => options.Filters.Add<NoelleFluentValidationFilter>())
    .ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true);
services.AddValidatorsFromAssemblyContaining<CreateTodoItemValidator>();

// 数据库 + 拦截器
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

// 工作单元与事件总线
services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddScoped<ITransactionManager, NoelleTransactionManager>();

var assemblies = new[] { typeof(Program).Assembly };
services.AddLocalEventBus(cfg =>
{
    cfg.RegisterServicesFromAssemblies(assemblies);
    cfg.UseMediatR(x => x.RegisterServicesFromAssemblies(assemblies));
});

// 仓储（显式注册）
services.AddScoped<ITodoItemRepository, TodoItemRepository>();
```

### 3. 使用

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

## 功能说明

- **异常处理**：`BusinessException` → 400，`EntityNotFoundException` → 404，自动转为 RFC 9457 ProblemDetails，支持错误码本地化；
- **模型验证**：`NoelleModelValidationFilter`（DataAnnotations）与 `NoelleFluentValidationFilter`（FluentValidation）二选一，统一错误格式；
- **领域事件**：由 `NoelleDomainEventInterceptor` 在 `SaveChanges` 提交前派发，处理器在事务内执行（禁止嵌套 `SaveChanges`，外部副作用请走分布式事件）；
- **分布式事件**：`PublishDelayAsync` 的延迟发布依赖 CAP 的持久化存储与调度器，InMemory 存储下无法保证生效。

---

## 文档与支持

- 注册骨架与快速入门见上文；更细的用法说明收录在仓库的 docs 目录中；
- 项目主页：[github.com/xiaolong233/noelle-net](https://github.com/xiaolong233/noelle-net)
- 目标框架 .NET 10 ｜ 许可证 MIT
