# Noelle.Net

面向 **.NET 9** 的应用基础类库，提供 DDD 领域模型、事件总线、EF Core 仓储、审计追踪、工作单元、ASP.NET Core 增强等开箱即用的组件。

[![NuGet](https://img.shields.io/badge/nuget-v9.1.4-blue)](https://www.nuget.org/)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com/)

---

## 特性

- **DDD 领域模型** — `Entity`、`AggregateRoot`、`ValueObject` 基类，内置领域事件
- **事件总线** — 本地事件（MediatR）+ 分布式事件（CAP），统一抽象接口
- **审计追踪** — 自动填充创建者/创建时间/修改者/修改时间
- **EF Core 仓储** — 通用 `EfCoreRepository<T>` 实现，内置审计/领域事件/GUID 拦截器
- **工作单元** — `IUnitOfWork` + `ITransactionManager`，统一事务管理
- **全局异常处理** — 自动转换为结构化 JSON 错误响应
- **FluentValidation** — 模型验证失败返回统一错误格式
- **安全主体** — `ICurrentUser` 统一访问当前用户信息
- **应用 DTO** — 内置分页、排序、列表结果等通用对象

---

## 安装

```bash
# 核心
dotnet add package NoelleNet.Core
dotnet add package NoelleNet.Ddd.Domain
dotnet add package NoelleNet.EntityFrameworkCore

# 事件总线
dotnet add package NoelleNet.EventBus
dotnet add package NoelleNet.EventBus.Local.MediatR
dotnet add package NoelleNet.EventBus.Distributed.CAP

# ASP.NET Core
dotnet add package NoelleNet.AspNetCore

# 其他
dotnet add package NoelleNet.Application.Contracts
dotnet add package NoelleNet.Extensions.MediatR
```

---

## 快速开始

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

## 包列表

| 包名 | 说明 |
|------|------|
| `NoelleNet.Core` | 基础库：异常、安全、序列化、缓存、工具类 |
| `NoelleNet.Ddd.Domain` | DDD 领域模型：实体、聚合根、值对象、领域事件 |
| `NoelleNet.EntityFrameworkCore` | EF Core 仓储、工作单元、审计/领域事件拦截器 |
| `NoelleNet.EventBus` | 事件总线抽象：本地 + 分布式 |
| `NoelleNet.EventBus.Local.MediatR` | 基于 MediatR 的本地事件总线实现 |
| `NoelleNet.EventBus.Distributed.CAP` | 基于 CAP 的分布式事件总线实现 |
| `NoelleNet.AspNetCore` | 全局异常、验证、SnakeCase 路由、认证错误响应 |
| `NoelleNet.Application.Contracts` | 应用层 DTO：分页、排序、列表结果 |
| `NoelleNet.Extensions.MediatR` | MediatR 管道行为：日志、事务管理 |
| `NoelleNet.Auditing` | 审计接口定义 |
| `NoelleNet.Uow` | 工作单元与事务管理器接口 |
| `NoelleNet.Extensions.CAP.SqlServer` | CAP + SQL Server 事务管理器 |
| `NoelleNet.Extensions.CAP.MySql` | CAP + MySQL 事务管理器 |
| `NoelleNet.Extensions.CAP.PostgreSql` | CAP + PostgreSQL 事务管理器 |

---

## 架构总览

```
NoelleNet.Core                   基础库
 ├─ NoelleNet.Auditing           审计接口
 ├─ NoelleNet.Uow                工作单元接口
 ├─ NoelleNet.Ddd.Domain         DDD 领域模型
 ├─ NoelleNet.EventBus           事件总线抽象
 │   ├─ Local.MediatR            MediatR 实现
 │   └─ Distributed.CAP          CAP 实现
 ├─ NoelleNet.EntityFrameworkCore EF Core 集成
 ├─ NoelleNet.AspNetCore         ASP.NET Core 增强
 ├─ NoelleNet.Application.Contracts  应用 DTO
 └─ NoelleNet.Extensions.*       扩展包
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

## 贡献

欢迎通过 [GitHub Issues](https://github.com/xiaolong233/noelle-net/issues) 提交 Bug 或功能建议，也欢迎 Pull Request。

```powershell
# 构建
dotnet build

# 打包
cd nupkg && ./pack.ps1
```

---

## 许可证

[MIT](LICENSE) © 2024 [xiaolong233](https://github.com/xiaolong233)
