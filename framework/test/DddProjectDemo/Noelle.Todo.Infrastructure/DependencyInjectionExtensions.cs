﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Noelle.Todo.Domain.Todo.Entities;
using Noelle.Todo.Infrastructure.Repositories;
using NoelleNet.Auditing.EntityFrameworkCore;

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
            options.AddInterceptors(serviceProvider.GetRequiredService<NoelleAuditInterceptor<long>>());
        }, ServiceLifetime.Scoped);

        // EF Core拦截器
        services.AddScoped<NoelleAuditInterceptor<long>>();

        // 仓储配置
        services.AddScoped<ITodoItemRepository, TodoItemRepository>();

        return services;
    }
}
