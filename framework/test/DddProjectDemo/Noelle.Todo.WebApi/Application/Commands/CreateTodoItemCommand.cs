using Noelle.Todo.WebApi.Application.Models.TodoItems;

namespace Noelle.Todo.WebApi.Application.Commands;

/// <summary>
/// 创建待办事项的命令
/// </summary>
/// <param name="Name">事项名称</param>
public record CreateTodoItemCommand(string Name) : IRequest<EntityDto<Guid>>;