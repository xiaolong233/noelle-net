using NoelleNet.Ddd.Domain.Repositories;

namespace Noelle.Todo.Domain.Todo.Entities;

/// <summary>
/// 代办事项仓储接口
/// </summary>
public interface ITodoItemRepository : INoelleRepository<TodoItem>
{
    /// <summary>
    /// 根据标识符查询待办事项
    /// </summary>
    /// <param name="id">待办事项的标识符</param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    Task<TodoItem?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加一个新的待办事项
    /// </summary>
    /// <param name="todoItem">待办事项</param>
    /// <returns></returns>
    TodoItem AddTodoItem(TodoItem todoItem);

    /// <summary>
    /// 更新代办事项
    /// </summary>
    /// <param name="todoItem">待办事项</param>
    void UpdateTodoItem(TodoItem todoItem);

    /// <summary>
    /// 删除指定待办事项
    /// </summary>
    /// <param name="id">待办事项的标识符</param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
