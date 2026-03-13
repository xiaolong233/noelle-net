using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Noelle.Todo.Domain.Todo;
using Noelle.Todo.Infrastructure.Repositories;
using NoelleNet.Auditing.EntityFrameworkCore;
using NoelleNet.EntityFrameworkCore.Interceptors;
using NoelleNet.EventBus.Distributed;
using NoelleNet.EventBus.Local;
using NoelleNet.Extensions.MediatR;
using NoelleNet.Uow;

namespace Noelle.Todo.Infrastructure.Extensions;

/// <summary>
/// 依赖注入的扩展方法集
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// 添加基础设施
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> 的实例</param>
    /// <param name="configuration"><see cref="IConfiguration"/> 的实例</param>
    /// <returns></returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
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
        services.AddDbContext<DbContext, TodoDbContext>((serviceProvider, options) =>
        {
            options.UseSqlServer(connectionString, options => options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                   .LogTo(Console.WriteLine, LogLevel.Information)
                   .EnableDetailedErrors();
            options.UseOpenIddict();

            // 添加拦截器
            options.AddInterceptors(serviceProvider.GetRequiredService<NoelleAutoSetGuidKeyInterceptor>());
            options.AddInterceptors(serviceProvider.GetRequiredService<NoelleAuditInterceptor>());
            options.AddInterceptors(serviceProvider.GetRequiredService<NoelleDomainEventInterceptor>());
        }, ServiceLifetime.Scoped);

        // 事件总线
        var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName?.Contains("Noelle.Todo") ?? false).ToArray();
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

        // 领域服务
        // services.AddScoped<XxxService>();

        return services;
    }
}
