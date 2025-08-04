using NoelleNet.EventBus.Abstractions.Distributed;

namespace Noelle.Todo.WebApi.Application.IntegrationEvents;

/// <summary>
/// <see cref="CreateTodoItemIntegrationEvent"/> 的处理程序
/// </summary>
public class CreateTodoItemIntegrationEventHandler : IDistributedEventHandler<CreateTodoItemIntegrationEvent>
{
    private readonly ILogger<CreateTodoItemIntegrationEventHandler> _logger;

    /// <summary>
    /// 创建一个新的 <see cref="CreateTodoItemIntegrationEventHandler"/> 实例
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <exception cref="ArgumentNullException"></exception>
    public CreateTodoItemIntegrationEventHandler(ILogger<CreateTodoItemIntegrationEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public Task HandleAsync(CreateTodoItemIntegrationEvent eventData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("接收到新的集成事件：{@event}", eventData);
        return Task.CompletedTask;
    }
}
