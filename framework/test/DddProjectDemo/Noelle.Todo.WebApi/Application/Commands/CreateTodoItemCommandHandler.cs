using MediatR;
using Noelle.Todo.Domain.Todo.Entities;
using Noelle.Todo.WebApi.Application.IntegrationEvents;
using Noelle.Todo.WebApi.Application.Models;
using NoelleNet.EventBus.Abstractions.Distributed;

namespace Noelle.Todo.WebApi.Application.Commands;

/// <summary>
/// 创建待办事项命令的处理器
/// </summary>
public class CreateTodoItemCommandHandler : IRequestHandler<CreateTodoItemCommand, TodoItemDto>
{
    private readonly ITodoItemRepository _repository;
    private readonly IDistributedEventBus _distributedEventBus;

    /// <summary>
    /// 创建一个新的 <see cref="CreateTodoItemCommandHandler"/> 实例
    /// </summary>
    /// <param name="repository">待办事项仓储</param>
    /// <param name="distributedEventBus">分布式事件总线</param>
    /// <exception cref="ArgumentNullException"></exception>
    public CreateTodoItemCommandHandler(ITodoItemRepository repository, IDistributedEventBus distributedEventBus)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _distributedEventBus = distributedEventBus ?? throw new ArgumentNullException(nameof(distributedEventBus));
    }

    /// <summary>
    /// 命令处理函数
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    public async Task<TodoItemDto> Handle(CreateTodoItemCommand request, CancellationToken cancellationToken)
    {
        var item = new TodoItem(request.Name);

        _repository.AddTodoItem(item);

        await _distributedEventBus.PublishAsync(new CreateTodoItemIntegrationEvent(item.Name), cancellationToken);

        //throw new Exception("发生异常了喵！");

        return new TodoItemDto(item.Id, item.Name, item.IsComplete);
    }
}
