namespace Noelle.Todo.WebApi.Application.Models.TodoItems;

/// <summary>
/// 更新待办事项的数据传输对象
/// </summary>
public record UpdateTodoItemDto
{
    /// <summary>
    /// 事项名称
    /// </summary>
    public string Name { get; init; } = default!;

    /// <summary>
    /// 是否已完成
    /// </summary>
    public bool IsComplete { get; init; }
}
