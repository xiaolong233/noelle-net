using MediatR;
using Noelle.Todo.Domain.Todo.Entities;
using Noelle.Todo.WebApi.Application.Models;
using NoelleNet.Ddd.Domain.Exceptions;

namespace Noelle.Todo.WebApi.Application.Commands;

/// <summary>
/// 修改待办事项名称的命令处理器
/// </summary>
/// <param name="repository">待办事项仓储实现</param>
public class ChangeTodoItemNameCommandHandler(ITodoItemRepository repository) : IRequestHandler<ChangeTodoItemNameCommand, TodoItemDto>
{
    private readonly ITodoItemRepository _repository = repository;

    /// <summary>
    /// 命令处理方法
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    public async Task<TodoItemDto> Handle(ChangeTodoItemNameCommand request, CancellationToken cancellationToken)
    {
        var item = await _repository.FindByIdAsync(request.Id, cancellationToken) ?? throw new NoelleEntityNotFoundException($"未找到id为[{request.Id}]的待办事项", typeof(TodoItem));
        item.ChangeName(request.NewName);
        _repository.UpdateTodoItem(item);

        return new TodoItemDto(item.Id, item.Name, item.IsComplete);
    }
}
