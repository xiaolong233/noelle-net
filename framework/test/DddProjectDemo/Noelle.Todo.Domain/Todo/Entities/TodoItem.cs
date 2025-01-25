using NoelleNet;
using NoelleNet.Auditing;
using NoelleNet.Ddd.Domain.Entities;
using NoelleNet.Ddd.Domain.Events;

namespace Noelle.Todo.Domain.Todo.Entities;

public class TodoItem : AuditableEntity<Guid, long>, IAggregateRoot
{
    public TodoItem(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("待办事项名称为空", nameof(name));

        Name = name;
        
        AddDomainEvent(new NoelleEntityCreatedEvent<TodoItem>(this));
    }

    /// <summary>
    /// 待办事项名称
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 是否已完成
    /// </summary>
    public bool IsComplete { get; private set; }

    /// <summary>
    /// 更改待办事项名称
    /// </summary>
    /// <param name="newName">新的待办事项名称</param>
    /// <exception cref="ArgumentException"></exception>
    public void ChangeName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("新的待办事项名称为空", nameof(newName));
        Name = newName;
        AddDomainEvent(new NoelleEntityChangedEvent<TodoItem>(this, EntityChangeType.Update));

        string? s = null;
        s.IsNullOrWhiteSpace();
    }

    /// <summary>
    /// 标记当前代办事项为已完成
    /// </summary>
    public void Complete()
    {
        this.IsComplete = true;
        AddDomainEvent(new NoelleEntityChangedEvent<TodoItem>(this, EntityChangeType.Update));
    }

    /// <summary>
    /// 重新开始当前事项
    /// </summary>
    public void Restart()
    {
        this.IsComplete = false;
        AddDomainEvent(new NoelleEntityChangedEvent<TodoItem>(this, EntityChangeType.Update));
    }
}
