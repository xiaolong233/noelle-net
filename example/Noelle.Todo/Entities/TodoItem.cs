using NoelleNet.Ddd.Domain.Entities.Auditing;
using NoelleNet.Ddd.Domain.Events;

namespace Noelle.Todo.Entities;

/// <summary>
/// 待办事项聚合根：包含创建/修改审计信息与领域事件
/// </summary>
public class TodoItem : AuditedAggregateRoot<Guid>
{
    /// <summary>
    /// EF Core 专用
    /// </summary>
    private TodoItem()
    {
    }

    public TodoItem(string title)
    {
        Id = Guid.NewGuid();
        SetTitle(title);
        AddDomainEvent(new EntityCreatedEvent<TodoItem>(this));
    }

    public string Title { get; private set; } = string.Empty;

    public bool IsCompleted { get; private set; }

    public void Complete()
    {
        IsCompleted = true;
        AddDomainEvent(new EntityUpdatedEvent<TodoItem>(this));
    }

    private void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("待办事项标题不能为空", nameof(title));

        Title = title;
    }
}
