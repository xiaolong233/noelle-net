using NoelleNet.Ddd.Domain.Events;

namespace Noelle.Todo.Events;

/// <summary>
/// 领域事件处理器：在 SaveChanges 提交前于事务内执行。
/// 契约：处理器内可以增删改实体（随本次保存一起提交），
/// 但禁止嵌套调用 SaveChanges，外部副作用请通过 IDistributedEventBus 发布集成事件。
/// </summary>
public class TodoItemCreatedEventHandler(ILogger<TodoItemCreatedEventHandler> logger)
    : IDomainEventHandler<EntityCreatedEvent<Entities.TodoItem>>
{
    public Task HandleAsync(EntityCreatedEvent<Entities.TodoItem> eventData, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("领域事件：待办事项 {TodoId}（{Title}）已创建", eventData.Entity.Id, eventData.Entity.Title);
        return Task.CompletedTask;
    }
}
