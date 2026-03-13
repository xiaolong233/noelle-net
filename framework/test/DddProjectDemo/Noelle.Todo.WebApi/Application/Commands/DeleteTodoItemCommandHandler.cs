using Noelle.Todo.Domain.Todo;

namespace Noelle.Todo.WebApi.Application.Commands;

/// <summary>
/// <see cref="DeleteTodoItemCommand"/> 的处理程序
/// </summary>
public class DeleteTodoItemCommandHandler : IRequestHandler<DeleteTodoItemCommand>
{
    private readonly ITodoItemRepository _repository;

    /// <summary>
    /// 创建一个新的 <see cref="DeleteTodoItemCommandHandler"/> 实例
    /// </summary>
    /// <param name="repository"><see cref="ITodoItemRepository"/> 实例</param>
    /// <exception cref="ArgumentNullException"></exception>
    public DeleteTodoItemCommandHandler(ITodoItemRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <inheritdoc/>
    public Task Handle(DeleteTodoItemCommand request, CancellationToken cancellationToken)
    {
        return _repository.RemoveByIdAsync(request.Id, cancellationToken);
    }
}
