namespace Noelle.Todo.WebApi.Application.Models;

/// <summary>
/// 代办事项传输对象
/// </summary>
/// <param name="Id">代办事项的标识</param>
/// <param name="Name">代办事项的名称</param>
/// <param name="IsComplete">是否已完成</param>
public record TodoItemDto(Guid Id, string Name, bool IsComplete);
