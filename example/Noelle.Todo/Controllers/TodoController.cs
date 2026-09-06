using Microsoft.AspNetCore.Mvc;
using NoelleNet;
using Noelle.Todo.Dtos;
using Noelle.Todo.Entities;
using Noelle.Todo.Services;

namespace Noelle.Todo.Controllers;

/// <summary>
/// 待办事项控制器
/// </summary>
[ApiController]
[Route("api/todo-items")]
public class TodoController(TodoAppService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TodoItemDto>> CreateAsync(CreateTodoInput input, CancellationToken cancellationToken)
    {
        var item = await service.CreateAsync(input, cancellationToken);
        return Created($"/api/todo-items/{item.Id}", item);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TodoItemDto>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await service.GetAsync(id, cancellationToken);
        if (item == null)
            throw new EntityNotFoundException(typeof(TodoItem), id);

        return Ok(item);
    }

    [HttpGet]
    public async Task<ActionResult<List<TodoItemDto>>> GetListAsync(CancellationToken cancellationToken)
        => Ok(await service.GetListAsync(cancellationToken));

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> CompleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await service.CompleteAsync(id, cancellationToken);
        return NoContent();
    }
}
