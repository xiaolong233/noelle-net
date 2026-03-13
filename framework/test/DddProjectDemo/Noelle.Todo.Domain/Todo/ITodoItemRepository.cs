namespace Noelle.Todo.Domain.Todo;

/// <summary>
/// 代办事项仓储接口
/// </summary>
public interface ITodoItemRepository : IRepository<TodoItem>
{
    /// <summary>
    /// 添加一个新的待办事项
    /// </summary>
    /// <param name="item">待办事项</param>
    /// <returns></returns>
    Task<TodoItem> AddTodoItemAsync(TodoItem item, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据标识符查询待办事项
    /// </summary>
    /// <param name="id">待办事项的标识符</param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    Task<TodoItem?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 移除一个待办事项
    /// </summary>
    /// <param name="item">待移除的待办事项</param>
    void Remove(TodoItem item);

    /// <summary>
    /// 移除指定标识符的待办事项
    /// </summary>
    /// <param name="id">待办事项的标识符</param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    Task RemoveByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
