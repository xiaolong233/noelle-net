using Noelle.Todo.WebApi.Application.Models.TodoItems;

namespace Noelle.Todo.WebApi.Application.Commands;

/// <summary>
/// 更新待办事项的命令
/// </summary>
/// <param name="Id">代办事项的标识符</param>
/// <param name="Data">更新待办事项的数据传输对象</param>
public record UpdateTodoItemCommand(Guid Id, UpdateTodoItemDto Data) : IRequest;