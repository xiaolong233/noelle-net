namespace Noelle.Todo.Dtos;

/// <summary>
/// 待办事项的响应模型（领域实体不直接暴露给 API）
/// </summary>
public record TodoItemDto(
    Guid Id,
    string Title,
    bool IsCompleted,
    DateTime CreatedAt,
    string? CreatedBy,
    DateTime? LastModifiedAt,
    string? LastModifiedBy)
{
    public static TodoItemDto FromEntity(Entities.TodoItem item) => new(
        item.Id,
        item.Title,
        item.IsCompleted,
        item.CreatedAt,
        item.CreatedBy,
        item.LastModifiedAt,
        item.LastModifiedBy);
}
