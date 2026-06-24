using MediatR;
using Moq;
using NoelleNet.EventBus.Abstractions.Local;

namespace NoelleNet.EventBus.Local.MediatR;

/// <summary>
/// <see cref="MediatRLocalEventBus"/> 的单元测试
/// </summary>
public class MediatRLocalEventBusTests
{
    #region Constructor

    /// <summary>
    /// 传入 null 的 IMediator 时应抛出 ArgumentNullException
    /// </summary>
    [Fact]
    public void Constructor_WithNullMediator_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MediatRLocalEventBus(null!));
    }

    /// <summary>
    /// 传入有效的 IMediator 时应成功创建实例
    /// </summary>
    [Fact]
    public void Constructor_WithValidMediator_ShouldCreateInstance()
    {
        var mockMediator = new Mock<IMediator>();
        var eventBus = new MediatRLocalEventBus(mockMediator.Object);
        Assert.NotNull(eventBus);
    }

    #endregion

    #region PublishAsync

    /// <summary>
    /// 传入 null 事件数据时应抛出 ArgumentNullException
    /// </summary>
    [Fact]
    public async Task PublishAsync_WithNullEventData_ShouldThrowArgumentNullException()
    {
        var mockMediator = new Mock<IMediator>();
        var eventBus = new MediatRLocalEventBus(mockMediator.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => eventBus.PublishAsync<TestEvent>(null!));
    }

    /// <summary>
    /// 发布有效事件时应使用正确的 LocalEventAdapter 调用 IMediator.Publish
    /// </summary>
    [Fact]
    public async Task PublishAsync_WithValidEvent_ShouldCallMediatorPublishWithAdapter()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        var eventBus = new MediatRLocalEventBus(mockMediator.Object);
        var eventData = new TestEvent { Id = 1, Name = "test" };

        // Act
        await eventBus.PublishAsync(eventData);

        // Assert
        mockMediator.Verify(
            m => m.Publish(
                It.Is<LocalEventAdapter>(a =>
                    a.SourceEvent == eventData &&
                    a.EventType == typeof(TestEvent)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// 发布事件时应将 CancellationToken 传递给 IMediator.Publish
    /// </summary>
    [Fact]
    public async Task PublishAsync_ShouldPassCancellationTokenToMediator()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        var eventBus = new MediatRLocalEventBus(mockMediator.Object);
        var eventData = new TestEvent { Id = 1, Name = "test" };
        var cancellationToken = new CancellationToken(true);

        // Act
        await eventBus.PublishAsync(eventData, cancellationToken);

        // Assert
        mockMediator.Verify(
            m => m.Publish(
                It.IsAny<LocalEventAdapter>(),
                cancellationToken),
            Times.Once);
    }

    /// <summary>
    /// 发布不同事件类型时应创建对应类型的 LocalEventAdapter
    /// </summary>
    [Fact]
    public async Task PublishAsync_WithDifferentEventTypes_ShouldCreateCorrectAdapter()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        var eventBus = new MediatRLocalEventBus(mockMediator.Object);
        var eventData = new AnotherTestEvent { Value = 42 };

        // Act
        await eventBus.PublishAsync(eventData);

        // Assert
        mockMediator.Verify(
            m => m.Publish(
                It.Is<LocalEventAdapter>(a =>
                    a.SourceEvent == eventData &&
                    a.EventType == typeof(AnotherTestEvent)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// 多次发布不同事件时应分别调用 IMediator.Publish
    /// </summary>
    [Fact]
    public async Task PublishAsync_WithMultipleEvents_ShouldPublishEachIndependently()
    {
        // Arrange
        var mockMediator = new Mock<IMediator>();
        var eventBus = new MediatRLocalEventBus(mockMediator.Object);
        var event1 = new TestEvent { Id = 1, Name = "first" };
        var event2 = new TestEvent { Id = 2, Name = "second" };

        // Act
        await eventBus.PublishAsync(event1);
        await eventBus.PublishAsync(event2);

        // Assert
        mockMediator.Verify(
            m => m.Publish(
                It.Is<LocalEventAdapter>(a => a.SourceEvent == event1),
                It.IsAny<CancellationToken>()),
            Times.Once);
        mockMediator.Verify(
            m => m.Publish(
                It.Is<LocalEventAdapter>(a => a.SourceEvent == event2),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Interface Implementation

    /// <summary>
    /// MediatRLocalEventBus 应实现 ILocalEventBus 接口
    /// </summary>
    [Fact]
    public void MediatRLocalEventBus_ShouldImplementILocalEventBus()
    {
        var mockMediator = new Mock<IMediator>();
        var eventBus = new MediatRLocalEventBus(mockMediator.Object);
        Assert.IsAssignableFrom<ILocalEventBus>(eventBus);
    }

    #endregion
}

#region Test Events

public class TestEvent
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class AnotherTestEvent
{
    public int Value { get; set; }
}

#endregion
