namespace Noelle.Todo.WebApi.Application.Models.TodoItems;

/// <summary>
/// 待办事项的数据传输对象
/// </summary>
public class TodoItemDto : AuditedEntityDto<Guid>
{
    /// <summary>
    /// 事项名称
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// 是否已完成
    /// </summary>
    public bool IsComplete { get; init; }
}
