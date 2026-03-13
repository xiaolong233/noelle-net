namespace Noelle.Todo.WebApi.Application.Models.TodoItems;

/// <summary>
/// 待办事项分页和排序的数据传输对象
/// </summary>
public class TodoItemPaginationAndSortDto : PaginationAndSortDto
{
    /// <summary>
    /// 事项名称
    /// </summary>
    public string? Name { get; init; }
}
