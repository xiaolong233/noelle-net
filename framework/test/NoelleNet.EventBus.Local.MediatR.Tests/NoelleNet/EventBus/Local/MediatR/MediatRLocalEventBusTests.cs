using MediatR;
using Moq;

namespace NoelleNet.EventBus.Local.MediatR;

/// <summary>
/// <see cref="MediatRLocalEventBus"/> 的契约测试：事件经 LocalEventAdapter 转发给 MediatR
/// </summary>
public class MediatRLocalEventBusTests
{
    /// <summary>
    /// 事件数据为 null 时应抛出 ArgumentNullException
    /// </summary>
    [Fact]
    public async Task PublishAsync_WithNullEventData_ShouldThrowArgumentNullException()
    {
        var eventBus = new MediatRLocalEventBus(new Mock<IMediator>().Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() => eventBus.PublishAsync<TestEvent>(null!));
    }

    /// <summary>
    /// 发布事件应构造携带源事件与运行时类型的 LocalEventAdapter 并调用 IMediator.Publish（含 CancellationToken 透传）
    /// </summary>
    [Fact]
    public async Task PublishAsync_ShouldForwardViaAdapterWithCancellationToken()
    {
        var mockMediator = new Mock<IMediator>();
        var eventBus = new MediatRLocalEventBus(mockMediator.Object);
        var eventData = new TestEvent { Id = 1, Name = "test" };
        var cancellationToken = new CancellationToken(true);

        await eventBus.PublishAsync(eventData, cancellationToken);

        mockMediator.Verify(
            m => m.Publish(
                It.Is<LocalEventAdapter>(a => a.SourceEvent == eventData && a.EventType == typeof(TestEvent)),
                cancellationToken),
            Times.Once);
    }

    /// <summary>
    /// 不同事件类型应分别构造对应类型的适配器（多次发布互不影响）
    /// </summary>
    [Fact]
    public async Task PublishAsync_WithDifferentEventTypes_ShouldCreateCorrectAdapters()
    {
        var mockMediator = new Mock<IMediator>();
        var eventBus = new MediatRLocalEventBus(mockMediator.Object);
        var event1 = new TestEvent { Id = 1 };
        var event2 = new AnotherTestEvent { Value = 42 };

        await eventBus.PublishAsync(event1);
        await eventBus.PublishAsync(event2);

        mockMediator.Verify(
            m => m.Publish(It.Is<LocalEventAdapter>(a => a.EventType == typeof(TestEvent)), It.IsAny<CancellationToken>()),
            Times.Once);
        mockMediator.Verify(
            m => m.Publish(It.Is<LocalEventAdapter>(a => a.EventType == typeof(AnotherTestEvent)), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

public class TestEvent
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class AnotherTestEvent
{
    public int Value { get; set; }
}
