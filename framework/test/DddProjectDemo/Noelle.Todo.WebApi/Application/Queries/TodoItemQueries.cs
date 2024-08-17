using Microsoft.EntityFrameworkCore;
using Noelle.Todo.Domain.Todo.Entities;
using Noelle.Todo.Infrastructure;
using Noelle.Todo.WebApi.Application.Models;
using NoelleNet.AspNetCore.Queries;

namespace Noelle.Todo.WebApi.Application.Queries;

/// <summary>
/// 待办事项查询器
/// </summary>
/// <remarks>
/// 构造函数
/// </remarks>
/// <param name="context"></param>
public class TodoItemQueries(TodoDbContext context) : ITodoItemQueries
{
    private readonly TodoDbContext _context = context;

    /// <summary>
    /// 获取待办事项
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<TodoItemDto?> GetTodoItemAsync(Guid id)
    {
        TodoItem? todoItem = await _context.TodoItems.FirstOrDefaultAsync(s => s.Id == id);
        return todoItem == null ? null : new TodoItemDto(todoItem.Id, todoItem.Name, todoItem.IsComplete);
    }

    /// <summary>
    /// 获取待办事项列表
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<NoellePaginationAndSortResultDto<TodoItemDto>> GetTodoItemsAsync(NoellePaginationAndSortDto dto)
    {
        IQueryable<TodoItem> query = _context.TodoItems;

        int total = await query.CountAsync();
        var items = await query.OrderByIf(!string.IsNullOrWhiteSpace(dto.Sort), dto.Sort)
                               .Skip(dto.Offset)
                               .Take(dto.Limit)
                               .Select(s => new TodoItemDto(s.Id, s.Name, s.IsComplete))
                               .ToListAsync();

        return new NoellePaginationAndSortResultDto<TodoItemDto>(total, items);
    }
}
