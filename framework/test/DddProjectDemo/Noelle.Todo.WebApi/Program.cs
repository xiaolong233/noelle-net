using Noelle.Todo.Infrastructure;
using Noelle.Todo.WebApi;
using Serilog;
using Serilog.Formatting.Compact;

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
    var builder = WebApplication.CreateBuilder(args);

    // 启用Serilog日志
    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplication(builder.Configuration);

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    // 身份启用身份认证和授权
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception e)
{
    Log.Error(e, "主机启动过程中发生异常！");
}
finally
{
    Log.CloseAndFlush();
}