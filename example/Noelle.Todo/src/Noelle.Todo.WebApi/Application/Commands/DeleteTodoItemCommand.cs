using MediatR;

namespace Noelle.Todo.WebApi.Application.Commands;

/// <summary>
/// 删除待办事项命令
/// </summary>
/// <param name="Id">待办事项的标识</param>
public record DeleteTodoItemCommand(Guid Id) : IRequest;
