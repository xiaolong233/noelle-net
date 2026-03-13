using Microsoft.EntityFrameworkCore;
using Noelle.Todo.Domain.Todo;
using Noelle.Todo.WebApi.Application.Models.TodoItems;
using System.Linq.Dynamic.Core;

namespace Noelle.Todo.WebApi.Application.Queries.TodoItems;

/// <summary>
/// <see cref="ITodoItemQueries"/> 的默认实现
/// </summary>
public class TodoItemQueries : ITodoItemQueries
{
    private readonly TodoDbContext _dbContext;

    /// <summary>
    /// 创建一个新的 <see cref="TodoItemQueries"/> 实例
    /// </summary>
    /// <param name="dbContext"><see cref="TodoDbContext"/> 实例</param>
    /// <exception cref="ArgumentNullException"></exception>
    public TodoItemQueries(TodoDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <inheritdoc/>
    public async Task<TodoItemDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _dbContext.TodoItems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw new EntityNotFoundException<TodoItem>(id);

        return new TodoItemDto
        {
            Id = item.Id,
            Name = item.Name,
            IsComplete = item.IsComplete,
            CreatedAt = item.CreatedAt,
            CreatedBy = item.CreatedBy,
            LastModifiedAt = item.LastModifiedAt,
            LastModifiedBy = item.LastModifiedBy,
        };
    }

    /// <inheritdoc/>
    public async Task<PaginationResultDto<TodoItemDto>> GetListAsync(TodoItemPaginationAndSortDto dto, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.TodoItems
            .AsNoTracking()
            .WhereIf(!string.IsNullOrWhiteSpace(dto.Name), x => x.Name.Contains(dto.Name!));

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(string.IsNullOrWhiteSpace(dto.Sort) ? $"{nameof(TodoItem.CreatedAt)} DESC" : dto.Sort)
            .Skip(dto.Offset)
            .Take(dto.Limit)
            .Select(x => new TodoItemDto
            {
                Id = x.Id,
                Name = x.Name,
                IsComplete = x.IsComplete,
                CreatedAt = x.CreatedAt,
                CreatedBy = x.CreatedBy,
                LastModifiedAt = x.LastModifiedAt,
                LastModifiedBy = x.LastModifiedBy,
            })
            .ToListAsync(cancellationToken);

        return new PaginationResultDto<TodoItemDto>(totalCount, items);
    }
}
