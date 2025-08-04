using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Noelle.Todo.Domain.Todo.Entities;
using Noelle.Todo.Infrastructure.Repositories;
using NoelleNet;
using NoelleNet.Auditing.EntityFrameworkCore;
using NoelleNet.EntityFrameworkCore.Interceptors;
using NoelleNet.Uow;

namespace Noelle.Todo.Infrastructure;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 数据库配置
        string connectionString = configuration.GetRequiredConnectionString("Default");
        services.AddDbContext<DbContext, TodoDbContext>((serviceProvider, options) =>
        {
            options.UseSqlServer(connectionString)
                   .LogTo(Console.WriteLine, LogLevel.Information)
                   .EnableDetailedErrors();
            options.UseOpenIddict();

            // 添加拦截器
            options.AddInterceptors(serviceProvider.GetRequiredService<NoelleAutoSetGuidKeyInterceptor>());
            options.AddInterceptors(serviceProvider.GetRequiredService<NoelleAuditInterceptor<long>>());
            options.AddInterceptors(serviceProvider.GetRequiredService<NoelleDomainEventInterceptor>());
        }, ServiceLifetime.Scoped);

        // EF Core拦截器
        services.AddScoped<NoelleAutoSetGuidKeyInterceptor>();
        services.AddScoped<NoelleAuditInterceptor<long>>();
        services.AddScoped<NoelleDomainEventInterceptor>();

        // 工作单元
        services.AddScoped<IUnitOfWork, NoelleUnitOfWork>();
        services.AddScoped<ITransactionManager, NoelleCapTransactionManager>();

        // 仓储配置
        services.AddScoped<ITodoItemRepository, TodoItemRepository>();

        // 通用服务配置
        services.AddSingleton<IGuidGenerator, NoelleGuidGenerator>();

        return services;
    }
}
