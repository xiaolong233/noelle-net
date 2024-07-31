using DotNetCore.CAP;
using MediatR;
using Noelle.Todo.Domain.Todo.Entities;
using Noelle.Todo.WebApi.Application.IntegrationEvents;
using Noelle.Todo.WebApi.Application.Models;

namespace Noelle.Todo.WebApi.Application.Commands;

/// <summary>
/// 创建待办事项命令的处理器
/// </summary>
/// <param name="repository">代办事项仓储的实例</param>
/// <param name="capPublisher">分布式事件总线实例</param>
public class CreateTodoItemCommandHandler(ITodoItemRepository repository, ICapPublisher capPublisher) : IRequestHandler<CreateTodoItemCommand, TodoItemDto>
{
    private readonly ITodoItemRepository _repository = repository;
    private readonly ICapPublisher _capPublisher = capPublisher;

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

        await _capPublisher.PublishAsync("todo.create_todo_item", new CreateTodoItemIntegrationEvent(item.Name));

        //throw new Exception("发生异常了喵！");

        return new TodoItemDto(item.Id, item.Name, item.IsComplete);
    }
}
