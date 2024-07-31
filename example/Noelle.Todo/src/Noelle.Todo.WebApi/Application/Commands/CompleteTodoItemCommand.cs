using MediatR;
using Noelle.Todo.WebApi.Application.Models;

namespace Noelle.Todo.WebApi.Application.Commands;

/// <summary>
/// 完成代办事项命令
/// </summary>
/// <param name="Id">待办事项的标识符</param>
public record CompleteTodoItemCommand(Guid Id) : IRequest;