using MediatR;
using Noelle.Todo.Domain.Todo.Entities;
using NoelleNet.Ddd.Domain.Exceptions;

namespace Noelle.Todo.WebApi.Application.Commands;

/// <summary>
/// 完成代办事项命令的处理器
/// </summary>
/// <param name="repository">代办事项仓储的实例</param>
public class CompleteTodoItemCommandHandler(ITodoItemRepository repository) : IRequestHandler<CompleteTodoItemCommand>
{
    private readonly ITodoItemRepository _repository = repository;

    /// <summary>
    /// 处理命令
    /// </summary>
    /// <param name="request">完成代办事项命令的实例</param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    /// <exception cref="NoelleEntityNotFoundException"></exception>
    public async Task Handle(CompleteTodoItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _repository.FindByIdAsync(request.Id, cancellationToken) ?? throw new NoelleEntityNotFoundException($"未找到id为[{request.Id}]的待办事项");
        item.Complete();
        _repository.UpdateTodoItem(item);
    }
}
