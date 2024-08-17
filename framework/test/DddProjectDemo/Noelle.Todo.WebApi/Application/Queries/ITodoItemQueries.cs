using Noelle.Todo.WebApi.Application.Models;
using NoelleNet.AspNetCore.Queries;

namespace Noelle.Todo.WebApi.Application.Queries;

/// <summary>
/// 定义待办事项查询器的功能
/// </summary>
public interface ITodoItemQueries
{
    /// <summary>
    /// 获取待办事项列表
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    Task<NoellePaginationResultDto<TodoItemDto>> GetTodoItemsAsync(NoellePaginationAndSortDto dto);

    /// <summary>
    /// 获取待办事项
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<TodoItemDto?> GetTodoItemAsync(Guid id);
}

