using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Noelle.Todo.Domain.Todo.Entities;
using Noelle.Todo.Infrastructure.Repositories;

namespace Noelle.Todo.Infrastructure;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 数据库配置
        string connectionString = configuration.GetRequiredConnectionString("Default");
        string versionString = configuration.GetRequiredValue("MySql:Version");
        var serverVersion = new MySqlServerVersion(versionString);
        services.AddDbContext<DbContext, TodoDbContext>(options =>
        {
            options.UseMySql(connectionString, serverVersion)
                   .LogTo(Console.WriteLine, LogLevel.Information)
                   .EnableSensitiveDataLogging()
                   .EnableDetailedErrors();
        }, ServiceLifetime.Scoped);

        // 仓储配置
        services.AddScoped<ITodoItemRepository, TodoItemRepository>();

        return services;
    }
}
