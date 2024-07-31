using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Noelle.Todo.Infrastructure;
using Noelle.Todo.DbMigratior;
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

    // 添加MediatR
    builder.Services.AddMediatR(options =>
    {
        options.RegisterServicesFromAssembly(Assembly.GetCallingAssembly());
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