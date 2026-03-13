using Noelle.Todo.Domain.Todo;

namespace Noelle.Todo.WebApi.Application.Commands;

/// <summary>
/// <see cref="UpdateTodoItemCommand"/> 的处理程序
/// </summary>
public class UpdateTodoItemCommandHandler : IRequestHandler<UpdateTodoItemCommand>
{
    private readonly ITodoItemRepository _repository;

    /// <summary>
    /// 创建一个新的 <see cref="UpdateTodoItemCommandHandler"/> 实例
    /// </summary>
    /// <param name="repository"><see cref="ITodoItemRepository"/> 实例</param>
    /// <exception cref="ArgumentNullException"></exception>
    public UpdateTodoItemCommandHandler(ITodoItemRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <inheritdoc/>
    public async Task Handle(UpdateTodoItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _repository.FindByIdAsync(request.Id, cancellationToken) ?? throw new EntityNotFoundException<TodoItem>(request.Id);

        item.ChangeName(request.Data.Name);
        if (request.Data.IsComplete)
            item.Complete();
        else
            item.Restart();
    }
}
