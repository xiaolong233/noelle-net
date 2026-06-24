using MediatR;

namespace NoelleNet.EventBus.Local.MediatR;

/// <summary>
/// <see cref="LocalEventAdapter"/> 的单元测试
/// </summary>
public class LocalEventAdapterTests
{
    #region Constructor

    /// <summary>
    /// 传入 null 的 sourceEvent 时应抛出 ArgumentNullException
    /// </summary>
    [Fact]
    public void Constructor_WithNullSourceEvent_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new LocalEventAdapter(null!));
    }

    /// <summary>
    /// 传入有效事件时应成功创建实例
    /// </summary>
    [Fact]
    public void Constructor_WithValidSourceEvent_ShouldCreateInstance()
    {
        var sourceEvent = new { Id = 1 };
        var adapter = new LocalEventAdapter(sourceEvent);
        Assert.NotNull(adapter);
    }

    #endregion

    #region SourceEvent

    /// <summary>
    /// SourceEvent 属性应返回构造函数中传入的源事件
    /// </summary>
    [Fact]
    public void SourceEvent_ShouldReturnOriginalEvent()
    {
        var sourceEvent = new TestAdapterEvent { Message = "hello" };
        var adapter = new LocalEventAdapter(sourceEvent);
        Assert.Same(sourceEvent, adapter.SourceEvent);
    }

    #endregion

    #region EventType

    /// <summary>
    /// EventType 属性应返回 sourceEvent 的类型
    /// </summary>
    [Fact]
    public void EventType_ShouldReturnCorrectType()
    {
        var sourceEvent = new TestAdapterEvent { Message = "test" };
        var adapter = new LocalEventAdapter(sourceEvent);
        Assert.Equal(typeof(TestAdapterEvent), adapter.EventType);
    }

    /// <summary>
    /// 不同事件类型应返回对应的 EventType
    /// </summary>
    [Fact]
    public void EventType_WithDifferentEventTypes_ShouldReturnCorrectTypes()
    {
        var event1 = new TestAdapterEvent { Message = "first" };
        var event2 = new AnotherAdapterEvent { Count = 42 };

        var adapter1 = new LocalEventAdapter(event1);
        var adapter2 = new LocalEventAdapter(event2);

        Assert.Equal(typeof(TestAdapterEvent), adapter1.EventType);
        Assert.Equal(typeof(AnotherAdapterEvent), adapter2.EventType);
    }

    #endregion

    #region Interface Implementation

    /// <summary>
    /// LocalEventAdapter 应实现 INotification 接口
    /// </summary>
    [Fact]
    public void ShouldImplementINotification()
    {
        var sourceEvent = new { Id = 1 };
        var adapter = new LocalEventAdapter(sourceEvent);
        Assert.IsAssignableFrom<INotification>(adapter);
    }

    #endregion
}

#region Test Events

public class TestAdapterEvent
{
    public string Message { get; set; } = string.Empty;
}

public class AnotherAdapterEvent
{
    public int Count { get; set; }
}

#endregion
