using MediatR;
using Noelle.Todo.Domain.Todo.Entities;
using NoelleNet.Ddd.Domain.Events;

namespace Noelle.Todo.WebApi.Application.DomainEvents;

/// <summary>
/// 创建新的待办事项的领域事件处理器
/// </summary>
/// <param name="logger"></param>
public class CreateTodoItemDomainEventHandler(ILogger<CreateTodoItemDomainEventHandler> logger) : IDomainEventHandler<NoelleEntityCreatedEvent<TodoItem>>
{
    private readonly ILogger<CreateTodoItemDomainEventHandler> _logger = logger;

    /// <inheritdoc/>
    public Task HandleAsync(NoelleEntityCreatedEvent<TodoItem> eventData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("创建了新的代办事项：[{Name}]", eventData.Entity.Name);
        return Task.FromResult(0);
    }
}
