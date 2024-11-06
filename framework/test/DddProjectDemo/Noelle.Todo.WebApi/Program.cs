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

    // ����Serilog��־
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

    // ������������֤����Ȩ
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception e)
{
    Log.Error(e, "�������������з����쳣��");
}
finally
{
    Log.CloseAndFlush();
}