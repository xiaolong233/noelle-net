using System.Diagnostics.CodeAnalysis;

namespace NoelleNet.EventBus.Abstractions;

/// <summary>
/// 用于标记事件名称
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class EventNameAttribute : Attribute
{
    /// <summary>
    /// 创建一个新的 <see cref="EventNameAttribute"/> 实例
    /// </summary>
    /// <param name="name">事件名称</param>
    /// <exception cref="ArgumentNullException"></exception>
    public EventNameAttribute([NotNull] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        Name = name;
    }

    /// <summary>
    /// 事件名称
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 分组名称
    /// </summary>
    public string? Group { get; set; } = default;
}
