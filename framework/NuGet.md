# Noelle.Net

Noelle.Net 是一个面向 **.NET 9** 的应用基础类库，提供 DDD（领域驱动设计）实体模型、事件总线、EF Core 仓储、审计追踪、工作单元、ASP.NET Core 增强等常用组件。

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

# CAP 数据库提供程序
dotnet add package NoelleNet.Extensions.CAP.SqlServer
dotnet add package NoelleNet.Extensions.CAP.MySql
dotnet add package NoelleNet.Extensions.CAP.PostgreSql

# MediatR 管道行为扩展
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
| **NoelleNet.AspNetCore** | ASP.NET Core 增强：全局异常处理、FluentValidation 模型验证、路由 snake_case 转换、认证授权错误响应 |
| **NoelleNet.Application.Contracts** | 应用层 DTO：分页/排序/列表结果、审计 DTO 基类 |
| **NoelleNet.Extensions.MediatR** | MediatR 管道行为：日志记录、事务自动管理、空中介者占位 |
| **NoelleNet.Extensions.CAP.SqlServer** | CAP + SQL Server 事务管理器 |
| **NoelleNet.Extensions.CAP.MySql** | CAP + MySQL 事务管理器 |
| **NoelleNet.Extensions.CAP.PostgreSql** | CAP + PostgreSQL 事务管理器 |

---

## 架构分层

```
NoelleNet.Core                        基础库
 ├─ NoelleNet.Auditing                审计接口
 ├─ NoelleNet.Uow                     工作单元接口
 ├─ NoelleNet.Ddd.Domain              DDD 领域模型
 │   ├─ Entity / AggregateRoot        实体与聚合根基类
 │   ├─ ValueObject                   值对象基类
 │   ├─ DomainEvents                  领域事件（创建/更新/删除）
 │   └─ IRepository                   仓储接口
 ├─ NoelleNet.EventBus                事件总线抽象
 │   ├─ ILocalEventBus                本地事件总线
 │   ├─ IDistributedEventBus          分布式事件总线
 │   ├─ Local.MediatR                 MediatR 实现
 │   └─ Distributed.CAP               CAP 实现
 ├─ NoelleNet.EntityFrameworkCore     EF Core 集成
 │   ├─ EfCoreRepository              仓储实现
 │   ├─ UnitOfWork / TransactionManager  工作单元/事务实现
 │   └─ Interceptors                  审计、领域事件、GUID 拦截器
 ├─ NoelleNet.AspNetCore              ASP.NET Core 增强
 │   ├─ 全局异常处理
 │   ├─ FluentValidation 验证
 │   ├─ SnakeCase 路由
 │   └─ 认证/授权错误响应
 ├─ NoelleNet.Application.Contracts   应用 DTO
 ├─ NoelleNet.Extensions.MediatR      MediatR 扩展
 └─ NoelleNet.Extensions.CAP.*       CAP 提供程序扩展
```

---

## 快速入门

### 1. 定义领域模型

```csharp
public class TodoItem : AuditedAggregateRoot<Guid>
{
    public TodoItem(string title)
    {
        Id = Guid.NewGuid();
        Title = title;
        AddDomainEvent(new EntityCreatedEvent<TodoItem>(this));
    }

    public string Title { get; private set; }
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
    Task<TodoItem> Addsync(TodoItem item, CancellationToken cancellationToken = default);
    Task<TodoItem?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Remove(TodoItem item);
    Task RemoveByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

// 实现
public class TodoItemRepository : EfCoreRepository<TodoItem, AppDbContext>, ITodoItemRepository
{
    public TodoItemRepository(TodoDbContext dbContext) : base(dbContext)
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
    {
        return DbContext.TodoItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <inheritdoc/>
    public void Remove(TodoItem item)
    {
        DbContext.TodoItems.Remove(item);
    }

    /// <inheritdoc/>
    public async Task RemoveByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await FindByIdAsync(id, cancellationToken);
        if(item == null)
            return;
        Remove(item);
    }
}
```

### 3. 配置 DbContext

```csharp
public class AppDbContext : DbContext
{
    public DbSet<TodoItem> TodoItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
```

### 4. 注册服务

```csharp
// 通用服务配置
services.AddSingleton<IGuidGenerator, NoelleGuidGenerator>();

// 工作单元
services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddScoped<ITransactionManager, NoelleCapTransactionManager>();

// EF Core拦截器
services.AddScoped<NoelleAutoSetGuidKeyInterceptor>();
services.AddScoped<NoelleAuditInterceptor>();
services.AddScoped<NoelleDomainEventInterceptor>();

// 数据库配置
string connectionString = configuration.GetRequiredConnectionString("Default");
services.AddDbContext<DbContext, AppDbContext>((sp, options) =>
{
    options.UseSqlServer(connectionString, options => options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
           .LogTo(Console.WriteLine, LogLevel.Information)
           .EnableDetailedErrors();

    // 添加拦截器
    options.AddInterceptors(sp.GetRequiredService<NoelleAutoSetGuidKeyInterceptor>());
    options.AddInterceptors(sp.GetRequiredService<NoelleAuditInterceptor>());
    options.AddInterceptors(sp.GetRequiredService<NoelleDomainEventInterceptor>());
}, ServiceLifetime.Scoped);

// 事件总线
var assemblies = new[] { typeof(Program).Assembly };
services.AddLocalEventBus(cfg =>
{
    cfg.RegisterServicesFromAssemblies(assemblies);
    cfg.UseMediatR(x =>
    {
        x.RegisterServicesFromAssemblies(assemblies);

        // 添加行为管道
        x.AddOpenBehavior(typeof(NoelleLoggingBehavior<,>));
        x.AddOpenBehavior(typeof(NoelleTransactionBehavior<,>));
    });
});
services.AddDistributedEventBus(cfg =>
{
    cfg.RegisterServicesFromAssemblies(assemblies);
    cfg.UseCap(options =>
    {
        options.UseEntityFramework<TodoDbContext>();
        options.UseRabbitMQ(rb =>
        {
            rb.HostName = configuration.GetRequiredValue("RabbitMQ:Host");
            rb.Port = Convert.ToInt32(configuration.GetRequiredValue("RabbitMQ:Port"));
            rb.UserName = configuration.GetRequiredValue("RabbitMQ:UserName");
            rb.Password = configuration.GetRequiredValue("RabbitMQ:Password");
        });
        options.DefaultGroupName = configuration.GetRequiredValue("RabbitMQ:GroupName");
    });
});

// 仓储配置
services.AddScoped<ITodoItemRepository, TodoItemRepository>();

return services;
```

### 5. 使用

```csharp
public class TodoService
{
    private readonly IRepository<TodoItem> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public TodoService(IRepository<TodoItem> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<TodoItem> CreateAsync(string title)
    {
        var item = new TodoItem(title);
        await _repository.AddAsync(item);
        await _unitOfWork.SaveChangesAsync();
        return item;
    }
}
```

---

## 异常处理

```csharp
// 业务异常
throw new BusinessException(
    errorCode: "TODO:DUPLICATE_NAME",
    message: "已存在同名的待办事项",
    details: "请更换待办事项名称后重试");

// 实体未找到
throw new EntityNotFoundException(typeof(TodoItem), itemId);
```

启用 `NoelleExceptionHandlingFilter` 后，异常自动转为结构化 JSON：

```json
{
  "error": {
    "code": "TODO:DUPLICATE_NAME",
    "message": "已存在同名的待办事项",
    "details": "请更换待办事项名称后重试",
    "traceId": "0HNA00ARMESN6"
  }
}
```

**全局异常处理与本地化**

在 `Program.cs` 中注册筛选器和本地化服务：

```csharp
builder.Services.AddControllers(options =>
{
    options.Filters.Add<NoelleExceptionHandlingFilter>();
});

// 启用错误代码本地化
builder.Services.Configure<NoelleExceptionLocalizationOptions>(options =>
{
    options.LocalizerProvider = (exception, factory) =>
        factory.Create(typeof(NoelleExceptionHandlingResource));
});
```

定义错误码对应的国际化资源文件（如 `NoelleExceptionHandlingResource.zh-CN.resx`）：

| Key | Value |
|-----|-------|
| `TODO:DUPLICATE_NAME` | 已存在同名的待办事项 "{title}" |

抛异常时通过 `Data` 传递占位参数：

```csharp
var ex = new BusinessException(errorCode: "TODO:DUPLICATE_NAME");
ex.Data["title"] = "买菜";
throw ex;
```

框架会自动根据 `ErrorCode` 查找本地化文本并替换 `{key}` 占位符。

---

## 适用范围

- **目标框架**: .NET 9
- **许可证**: MIT
- **仓库**: [github.com/xiaolong233/noelle-net](https://github.com/xiaolong233/noelle-net)
