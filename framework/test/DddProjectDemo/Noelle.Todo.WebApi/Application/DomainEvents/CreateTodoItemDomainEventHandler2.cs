using Noelle.Todo.Domain.Todo;
using NoelleNet.Ddd.Domain.Events;

namespace Noelle.Todo.WebApi.Application.DomainEvents;

public class CreateTodoItemDomainEventHandler2 : IDomainEventHandler<EntityCreatedEvent<TodoItem>>
{
    private readonly ILogger<CreateTodoItemDomainEventHandler> _logger;

    public CreateTodoItemDomainEventHandler2(ILogger<CreateTodoItemDomainEventHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public Task HandleAsync(EntityCreatedEvent<TodoItem> eventData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("创建了新的代办事项：[{Name}] {Handler} {Time}", eventData.Entity.Name, this.GetType().Name, DateTimeOffset.Now);
        return Task.FromResult(0);
    }
}
