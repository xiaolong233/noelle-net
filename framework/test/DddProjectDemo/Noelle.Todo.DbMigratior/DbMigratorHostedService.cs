using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Noelle.Todo.Infrastructure;

namespace Noelle.Todo.DbMigratior;

public class DbMigratorHostedService(IHostApplicationLifetime applicationLifetime, IServiceProvider serviceProvider) : IHostedService
{
    private readonly IHostApplicationLifetime _applicationLifetime = applicationLifetime;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <summary>
    /// 服务开始
    /// </summary>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        TodoDbContext dbContext = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);

        _applicationLifetime.StopApplication();
    }

    /// <summary>
    /// 服务结束
    /// </summary>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
