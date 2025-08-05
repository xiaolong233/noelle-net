using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Noelle.Todo.DbMigratior;
using Noelle.Todo.Infrastructure;
using NoelleNet.EventBus.Local;
using NoelleNet.Security;
using NoelleNet.Security.Claims;
using Serilog;
using Serilog.Formatting.Compact;
using System.Reflection;

Log.Logger = new LoggerConfiguration()
#if DEBUG
    .MinimumLevel.Debug()
#else
    .MinimumLevel.Information()
#endif
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Async(s => s.Console())
    .WriteTo.Async(s => s.File(new CompactJsonFormatter(), "Logs/log-.txt", rollingInterval: RollingInterval.Day))
    .CreateLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);
    builder.Logging.AddSerilog();

    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddScoped<ICurrentUser, CurrentUser>();
    builder.Services.AddScoped<ICurrentPrincipalProvider, NoelleEmptyCurrentPrincipalProvider>();

    // 添加本地事件总线
    builder.Services.AddLocalEventBus(cfg =>
    {
        cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        cfg.UseMediatR(o =>
        {
            o.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });
    });

    // 添加主机服务
    builder.Services.AddHostedService<DbMigratorHostedService>();

    var app = builder.Build();

    await app.RunAsync();
}
catch (Exception e)
{
    Log.Error(e, "主机启动过程中发生异常！");
}
finally
{
    Log.CloseAndFlush();
}