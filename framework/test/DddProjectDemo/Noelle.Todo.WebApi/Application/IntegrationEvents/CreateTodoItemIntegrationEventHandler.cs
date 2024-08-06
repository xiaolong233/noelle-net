using DotNetCore.CAP;

namespace Noelle.Todo.WebApi.Application.IntegrationEvents;

/// <summary>
/// 
/// </summary>
/// <param name="logger"></param>
public class CreateTodoItemIntegrationEventHandler(ILogger<CreateTodoItemIntegrationEventHandler> logger) : ICapSubscribe
{
    private readonly ILogger<CreateTodoItemIntegrationEventHandler> _logger = logger;

    /// <summary>
    /// 事件处理
    /// </summary>
    /// <param name="event">事件信息实例</param>
    /// <returns></returns>
    [CapSubscribe("todo.create_todo_item")]
    public void Subscribe(CreateTodoItemIntegrationEvent @event)
    {
        _logger.LogInformation("接收到新的集成事件：{@event}", @event);
    }
}
