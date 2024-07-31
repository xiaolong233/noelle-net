namespace Noelle.Todo.WebApi.Application.Models;

/// <summary>
/// 修改待办事项名称的请求传输对象
/// </summary>
/// <param name="NewName">新的代办事项名称</param>
public record ChangeTodoItemNameRequestDto(string NewName);
