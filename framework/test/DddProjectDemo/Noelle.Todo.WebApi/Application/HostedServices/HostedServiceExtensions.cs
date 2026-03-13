namespace Noelle.Todo.WebApi.Application.HostedServices;

/// <summary>
/// 托管服务的扩展方法集
/// </summary>
public static class HostedServiceExtensions
{
    /// <summary>
    /// 添加托管服务
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> 实例</param>
    public static IServiceCollection AddHostedServices(this IServiceCollection services)
    {
        // 配置托管服务
        services.AddHealthChecks();
        services.AddHostedService<SeedDataHostedService>();

        return services;
    }
}