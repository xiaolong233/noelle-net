using Microsoft.EntityFrameworkCore;
using NoelleNet.Ddd.Domain.Repositories.EntityFrameworkCore;
using Noelle.Todo.Data;
using Noelle.Todo.Entities;

namespace Noelle.Todo.Repositories;

/// <summary>
/// 基于 EfCoreRepository 的待办事项仓储实现
/// </summary>
public class TodoItemRepository : EfCoreRepository<TodoItem, AppDbContext>, ITodoItemRepository
{
    public TodoItemRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    /// <inheritdoc/>
    public async Task<TodoItem> AddAsync(TodoItem item, CancellationToken cancellationToken = default)
    {
        var entry = await DbContext.TodoItems.AddAsync(item, cancellationToken);
        return entry.Entity;
    }

    /// <inheritdoc/>
    public Task<TodoItem?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => DbContext.TodoItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    /// <inheritdoc/>
    public Task<List<TodoItem>> GetListAsync(CancellationToken cancellationToken = default)
        => DbContext.TodoItems.OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public void Remove(TodoItem item) => DbContext.TodoItems.Remove(item);
}
