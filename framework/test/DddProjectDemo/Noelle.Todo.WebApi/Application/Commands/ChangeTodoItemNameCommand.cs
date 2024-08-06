using MediatR;
using Noelle.Todo.WebApi.Application.Models;

namespace Noelle.Todo.WebApi.Application.Commands;

/// <summary>
/// 修改待办事项名称的命令
/// </summary>
/// <param name="Id">待办事项的标识符</param>
/// <param name="NewName">新的待办事项名称</param>
public record ChangeTodoItemNameCommand(Guid Id, string NewName) : IRequest<TodoItemDto>;