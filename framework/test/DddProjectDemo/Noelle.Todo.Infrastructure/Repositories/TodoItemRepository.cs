using Noelle.Todo.Domain.Todo;

namespace Noelle.Todo.Infrastructure.Repositories;

/// <summary>
/// <see cref="ITodoItemRepository"/> 的默认实现
/// </summary>
public class TodoItemRepository : EfCoreRepository<TodoItem, TodoDbContext>, ITodoItemRepository
{
    /// <summary>
    /// 创建一个新的 <see cref="TodoItemRepository"/> 实例
    /// </summary>
    /// <param name="dbContext"><see cref="TodoDbContext"/> 实例</param>
    public TodoItemRepository(TodoDbContext dbContext) : base(dbContext)
    {
    }

    /// <inheritdoc/>
    public async Task<TodoItem> AddTodoItemAsync(TodoItem item, CancellationToken cancellationToken = default)
    {
        var entry = await DbContext.TodoItems.AddAsync(item, cancellationToken);

        return entry.Entity;
    }

    /// <inheritdoc/>
    public Task<TodoItem?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return DbContext.TodoItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <inheritdoc/>
    public void Remove(TodoItem item)
    {
        DbContext.TodoItems.Remove(item);
    }

    /// <inheritdoc/>
    public async Task RemoveByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await FindByIdAsync(id, cancellationToken) ?? throw new EntityNotFoundException<TodoItem>(id);

        Remove(item);
    }
}
