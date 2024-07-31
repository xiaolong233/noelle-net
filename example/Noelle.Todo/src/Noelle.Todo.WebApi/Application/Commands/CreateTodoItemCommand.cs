using MediatR;
using Noelle.Todo.WebApi.Application.Models;

namespace Noelle.Todo.WebApi.Application.Commands;

/// <summary>
/// 创建待办事项命令
/// </summary>
/// <param name="Name">代办事项的名称</param>
public record CreateTodoItemCommand(string Name) : IRequest<TodoItemDto>;