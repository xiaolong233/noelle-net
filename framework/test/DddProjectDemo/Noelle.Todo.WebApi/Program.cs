using Noelle.Todo.Infrastructure.Extensions;
using NoelleNet.AspNetCore.Authentication;
using NoelleNet.AspNetCore.Authorization;
using Scalar.AspNetCore;
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
    .WriteTo.Async(s => s.File(new CompactJsonFormatter(), "Logs/log-.txt", rollingInterval: RollingInterval.Hour, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error))
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // 启用Serilog日志
    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplication(builder.Configuration, builder.Environment);
    builder.Services.AddOpenApi(options => options.AddScalarTransformers());

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    app.UseHttpsRedirection();

    // 启用本地化中间件
    app.UseRequestLocalization();

    // 启用身份认证失败和授权失败时的错误响应中间件
    app.UseAuthenticationErrorResponse();
    app.UseAuthorizationErrorResponse();

    // 启用身份认证和授权中间件
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