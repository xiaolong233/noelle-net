using Noelle.Todo.WebApi.Application.Queries.TodoItems;

namespace Noelle.Todo.WebApi.Application.Queries;

/// <summary>
/// 数据查询的扩展方法集
/// </summary>
public static class QueriesExtensions
{
    /// <summary>
    /// 添加数据查询服务
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> 实例</param>
    /// <returns></returns>
    public static IServiceCollection AddQueies(this IServiceCollection services)
    {
        // 配置数据查询
        services.AddScoped<ITodoItemQueries, TodoItemQueries>();

        return services;
    }
}