namespace Noelle.Todo.Dtos;

/// <summary>
/// 创建待办事项的输入模型
/// </summary>
public class CreateTodoInput
{
    public string Title { get; set; } = string.Empty;
}
