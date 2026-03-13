using Noelle.Todo.WebApi.Application.Models.TodoItems;

namespace Noelle.Todo.WebApi.Application.Queries.TodoItems;

/// <summary>
/// 定义待办事项查询器的功能
/// </summary>
public interface ITodoItemQueries
{
    /// <summary>
    /// 获取待办事项列表
    /// </summary>
    /// <param name="dto">待办事项分页和排序的数据传输对象</param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    Task<PaginationResultDto<TodoItemDto>> GetListAsync(TodoItemPaginationAndSortDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定标识符的待办事项
    /// </summary>
    /// <param name="id">待办事项的标识符</param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    Task<TodoItemDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

