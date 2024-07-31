namespace Noelle.Todo.WebApi.Application.IntegrationEvents;

/// <summary>
/// 创建新的待办事项时触发的集成事件
/// </summary>
/// <param name="Name">代办事项名称</param>
public record CreateTodoItemIntegrationEvent(string Name);
