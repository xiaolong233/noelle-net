using MediatR;
using Noelle.Todo.Domain.Todo.Entities;

namespace Noelle.Todo.WebApi.Application.Commands;

/// <summary>
/// 删除待办事项命令的处理器
/// </summary>
/// <param name="repository">待办事项仓储的实例</param>
public class DeleteTodoItemCommandHandler(ITodoItemRepository repository) : IRequestHandler<DeleteTodoItemCommand>
{
    private readonly ITodoItemRepository _repository = repository;

    /// <summary>
    /// 命令处理
    /// </summary>
    /// <param name="request">删除待办事项命令的实例</param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    public Task Handle(DeleteTodoItemCommand request, CancellationToken cancellationToken)
    {
        return _repository.DeleteByIdAsync(request.Id, cancellationToken);
    }
}
