using Microsoft.EntityFrameworkCore;
using Noelle.Todo.Domain.Todo.Entities;
using NoelleNet.Ddd.Domain.Repositories.EntityFrameworkCore;

namespace Noelle.Todo.Infrastructure.Repositories;

public class TodoItemRepository(TodoDbContext dbContext) : NoelleEfCoreRepository<TodoItem, TodoDbContext>(dbContext), ITodoItemRepository
{
    public TodoItem AddTodoItem(TodoItem todoItem)
    {
        var entry = DbContext.TodoItems.Add(todoItem);
        return entry.Entity;
    }

    public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        TodoItem? item = await FindByIdAsync(id, cancellationToken);
        if (item == null)
            return;
        DbContext.TodoItems.Remove(item);
    }

    public Task<TodoItem?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return DbContext.TodoItems.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public void UpdateTodoItem(TodoItem todoItem)
    {
        DbContext.Entry(todoItem).State = EntityState.Modified;
    }
}
