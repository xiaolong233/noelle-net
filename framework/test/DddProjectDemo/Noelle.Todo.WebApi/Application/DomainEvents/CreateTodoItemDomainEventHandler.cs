using MediatR;
using Noelle.Todo.Domain.Todo;
using NoelleNet.Ddd.Domain.Events;

namespace Noelle.Todo.WebApi.Application.DomainEvents;

/// <summary>
/// 创建新的待办事项的领域事件处理器
/// </summary>
/// <param name="logger"></param>
public class CreateTodoItemDomainEventHandler(ILogger<CreateTodoItemDomainEventHandler> logger) : IDomainEventHandler<EntityCreatedEvent<TodoItem>>
{
    private readonly ILogger<CreateTodoItemDomainEventHandler> _logger = logger;

    /// <inheritdoc/>
    public async Task HandleAsync(EntityCreatedEvent<TodoItem> eventData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("创建了新的代办事项：[{Name}] {Handler} {Time}", eventData.Entity.Name, this.GetType().Name, DateTimeOffset.Now);
        await Task.Delay(3000, cancellationToken);
    }
}
