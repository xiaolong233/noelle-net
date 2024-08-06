using MediatR;
using Noelle.Todo.Domain.Todo.Entities;
using NoelleNet.Ddd.Domain.Events;

namespace Noelle.Todo.WebApi.Application.DomainEvents;

/// <summary>
/// 创建新的待办事项的领域事件处理器
/// </summary>
/// <param name="logger"></param>
public class CreateTodoItemDomainEventHandler(ILogger<CreateTodoItemDomainEventHandler> logger) : INotificationHandler<NoelleEntityCreatedEvent<TodoItem>>
{
    private readonly ILogger<CreateTodoItemDomainEventHandler> _logger = logger;

    /// <summary>
    /// 处理事件
    /// </summary>
    /// <param name="notification">事件的实例</param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    public Task Handle(NoelleEntityCreatedEvent<TodoItem> notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("创建了新的代办事项：[{Name}]", notification.Entity.Name);
        return Task.CompletedTask;
    }
}
