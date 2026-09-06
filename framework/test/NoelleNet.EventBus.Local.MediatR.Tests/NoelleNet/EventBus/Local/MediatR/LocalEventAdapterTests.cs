namespace NoelleNet.EventBus.Local.MediatR;

/// <summary>
/// <see cref="LocalEventAdapter"/> 的契约测试：承载源事件及其运行时类型
/// </summary>
public class LocalEventAdapterTests
{
    /// <summary>
    /// 构造时应保留源事件引用并记录运行时类型
    /// </summary>
    [Fact]
    public void Constructor_ShouldCarrySourceEventAndRuntimeType()
    {
        var sourceEvent = new TestAdapterEvent { Message = "hello" };

        var adapter = new LocalEventAdapter(sourceEvent);

        Assert.Same(sourceEvent, adapter.SourceEvent);
        Assert.Equal(typeof(TestAdapterEvent), adapter.EventType);
    }
}

public class TestAdapterEvent
{
    public string Message { get; set; } = string.Empty;
}
