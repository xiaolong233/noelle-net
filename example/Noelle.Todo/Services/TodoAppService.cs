using NoelleNet;
using NoelleNet.Uow;
using Noelle.Todo.Dtos;
using Noelle.Todo.Entities;
using Noelle.Todo.Repositories;

namespace Noelle.Todo.Services;

/// <summary>
/// 待办事项应用服务
/// </summary>
public class TodoAppService(ITodoItemRepository repository, IUnitOfWork unitOfWork)
{
    public async Task<TodoItemDto> CreateAsync(CreateTodoInput input, CancellationToken cancellationToken = default)
    {
        var item = new TodoItem(input.Title);
        await repository.AddAsync(item, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return TodoItemDto.FromEntity(item);
    }

    public async Task<TodoItemDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository.FindByIdAsync(id, cancellationToken);
        return item == null ? null : TodoItemDto.FromEntity(item);
    }

    public async Task<List<TodoItemDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var items = await repository.GetListAsync(cancellationToken);
        return items.Select(TodoItemDto.FromEntity).ToList();
    }

    public async Task CompleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository.FindByIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException(typeof(TodoItem), id);

        item.Complete();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
