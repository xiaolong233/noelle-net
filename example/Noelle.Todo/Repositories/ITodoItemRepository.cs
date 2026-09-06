using NoelleNet.Ddd.Domain.Repositories;
using Noelle.Todo.Entities;

namespace Noelle.Todo.Repositories;

/// <summary>
/// 待办事项仓储接口
/// </summary>
public interface ITodoItemRepository : IRepository<TodoItem>
{
    Task<TodoItem> AddAsync(TodoItem item, CancellationToken cancellationToken = default);

    Task<TodoItem?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<TodoItem>> GetListAsync(CancellationToken cancellationToken = default);

    void Remove(TodoItem item);
}
