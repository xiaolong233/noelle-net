using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoelleNet;
using NoelleNet.AspNetCore.ExceptionHandling;
using NoelleNet.AspNetCore.Security.Claims;
using NoelleNet.AspNetCore.Validation;
using NoelleNet.Auditing.EntityFrameworkCore;
using NoelleNet.EntityFrameworkCore.Interceptors;
using NoelleNet.EventBus.Local;
using NoelleNet.Security;
using NoelleNet.Security.Claims;
using NoelleNet.Uow;
using Noelle.Todo.Data;
using Noelle.Todo.Repositories;
using Noelle.Todo.Services;
using Noelle.Todo.Validators;

var builder = WebApplication.CreateBuilder(args);

// ===== 1. 安全主体：ICurrentUser（异常处理与审计拦截器依赖） =====
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ICurrentPrincipalProvider, NoelleHttpContextCurrentPrincipalProvider>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// ===== 2. GUID 生成器 =====
builder.Services.AddSingleton<IGuidGenerator, NoelleGuidGenerator>();

// ===== 3. 本地化 + ProblemDetails + 全局异常处理（.NET 8+ 推荐方式） =====
builder.Services.AddLocalization();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<IErrorResponseWriter, ProblemDetailsErrorResponseWriter>();
builder.Services.AddExceptionHandler<NoelleExceptionHandler>();

// ===== 4. 控制器 + 模型验证 =====
builder.Services.AddControllers(options =>
{
    // 两个验证过滤器底层机制不同，按实际情况二选一：
    // NoelleModelValidationFilter 基于 ModelState（DataAnnotations 绑定期验证）；
    // NoelleFluentValidationFilter 基于 FluentValidation（按参数类型解析 IValidator<T>）。
    options.Filters.Add<NoelleFluentValidationFilter>();
    // options.Filters.Add<NoelleModelValidationFilter>();
});
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    // 关闭 [ApiController] 内置验证短路，让验证错误统一走框架异常处理。
    // 使用 NoelleModelValidationFilter 时必须设置（否则过滤器永远不会执行）；
    // 仅使用 NoelleFluentValidationFilter 时也建议设置（否则绑定期错误仍输出内置格式）
    options.SuppressModelStateInvalidFilter = true;
});
builder.Services.AddValidatorsFromAssemblyContaining<CreateTodoInputValidator>();

// ===== 5. 数据库（示例使用 SQLite 免配置；生产可换 SqlServer/MySQL/PostgreSQL） =====
var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=todos.db";

// AddDbContext<DbContext, AppDbContext> 会同时注册抽象与具体两个服务类型：
// 框架组件（UnitOfWork 等）依赖抽象的 DbContext，仓储依赖具体的 AppDbContext，一次注册同时满足两者
builder.Services.AddDbContext<DbContext, AppDbContext>((sp, options) =>
{
    options.UseSqlite(connectionString);

    // 拦截器顺序：先设置键值与审计信息，最后派发领域事件
    options.AddInterceptors(
        sp.GetRequiredService<NoelleAutoSetGuidKeyInterceptor>(),
        sp.GetRequiredService<NoelleAuditInterceptor>(),
        sp.GetRequiredService<NoelleDomainEventInterceptor>());
});

// ===== 6. EF Core 拦截器 =====
builder.Services.AddScoped<NoelleAutoSetGuidKeyInterceptor>();
builder.Services.AddScoped<NoelleAuditInterceptor>();
builder.Services.AddScoped<NoelleDomainEventInterceptor>();

// ===== 7. 工作单元与事务管理器 =====
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITransactionManager, NoelleTransactionManager>();

// ===== 8. 事件总线（本地 MediatR；分布式 CAP 见下方注释） =====
var assemblies = new[] { typeof(Program).Assembly };
builder.Services.AddLocalEventBus(cfg =>
{
    cfg.RegisterServicesFromAssemblies(assemblies);
    cfg.UseMediatR(x => x.RegisterServicesFromAssemblies(assemblies));
});

// ===== 9. 仓储与应用服务（显式注册） =====
builder.Services.AddScoped<ITodoItemRepository, TodoItemRepository>();
builder.Services.AddScoped<TodoAppService>();

var app = builder.Build();

// 演示用：自动建表
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
}

app.UseExceptionHandler();
app.MapControllers();

app.Run();

// ===== 分布式事件总线（CAP）示例：需引用 NoelleNet.EventBus.Distributed.CAP 与对应存储/传输包 =====
// 注意：NoelleCapTransactionManager 位于 NoelleNet.Extensions.CAP.SqlServer/MySql/PostgreSql 包，
// 使用 CAP 事务发件箱时用它替换上面的 NoelleTransactionManager。
//
// builder.Services.AddScoped<ITransactionManager, NoelleCapTransactionManager>();
// builder.Services.AddDistributedEventBus(cfg =>
// {
//     cfg.RegisterServicesFromAssemblies(assemblies);
//     cfg.UseCap(options =>
//     {
//         options.UseEntityFramework<AppDbContext>();
//         options.UseRabbitMQ(rb =>
//         {
//             rb.HostName = builder.Configuration.GetRequiredValue("RabbitMQ:Host");
//             rb.Port = Convert.ToInt32(builder.Configuration.GetRequiredValue("RabbitMQ:Port"));
//             rb.UserName = builder.Configuration.GetRequiredValue("RabbitMQ:UserName");
//             rb.Password = builder.Configuration.GetRequiredValue("RabbitMQ:Password");
//         });
//     });
// });
