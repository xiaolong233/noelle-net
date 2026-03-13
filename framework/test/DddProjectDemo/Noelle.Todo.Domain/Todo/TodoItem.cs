using NoelleNet.Ddd.Domain.Entities.Auditing;
using NoelleNet.Ddd.Domain.Events;

namespace Noelle.Todo.Domain.Todo;

/// <summary>
/// 代办事项
/// </summary>
public class TodoItem : AuditedAggregateRoot<Guid>
{
    /// <summary>
    /// 创建一个新的 <see cref="TodoItem"/> 实例
    /// </summary>
    protected TodoItem()
    {
    }

    /// <summary>
    /// 创建一个新的 <see cref="TodoItem"/> 实例
    /// </summary>
    /// <param name="name">事项名称</param>
    /// <exception cref="BusinessException"></exception>
    public TodoItem(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessException(TodoDomainErrorCodes.TodoItemNameNull);

        Id = Guid.CreateVersion7();
        Name = name;
        IsComplete = false;

        AddDomainEvent(new EntityCreatedEvent<TodoItem>(this));
    }

    /// <summary>
    /// 事项名称
    /// </summary>
    public string Name { get; private set; } = default!;

    /// <summary>
    /// 是否已完成
    /// </summary>
    public bool IsComplete { get; private set; }

    #region 行为方法
    /// <summary>
    /// 更改待办事项名称
    /// </summary>
    /// <param name="newName">新的待办事项名称</param>
    /// <exception cref="ArgumentException"></exception>
    public void ChangeName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new BusinessException(TodoDomainErrorCodes.TodoItemNameNull);

        Name = newName;
        AddDomainEvent(new EntityChangedEvent<TodoItem>(this, EntityChangeType.Update));
    }

    /// <summary>
    /// 标记当前代办事项为已完成
    /// </summary>
    public void Complete()
    {
        IsComplete = true;

        AddDomainEvent(new EntityChangedEvent<TodoItem>(this, EntityChangeType.Update));
    }

    /// <summary>
    /// 重新开始当前事项
    /// </summary>
    public void Restart()
    {
        IsComplete = false;

        AddDomainEvent(new EntityChangedEvent<TodoItem>(this, EntityChangeType.Update));
    }
    #endregion
}
