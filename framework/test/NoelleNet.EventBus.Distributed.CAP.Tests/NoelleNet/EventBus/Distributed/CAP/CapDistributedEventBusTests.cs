using DotNetCore.CAP;
using Moq;
using NoelleNet.EventBus.Abstractions;
using NoelleNet.EventBus.Distributed.CAP;

namespace NoelleNet.EventBus.Distributed.CAP;

/// <summary>
/// <see cref="CapDistributedEventBus"/> 的单元测试
/// </summary>
public class CapDistributedEventBusTests
{
    #region PublishAsync

    /// <summary>
    /// 发布带有有效 EventNameAttribute 的事件时，应使用正确的事件名称调用 ICapPublisher.PublishAsync
    /// </summary>
    [Fact]
    public async Task PublishAsync_WithValidEvent_ShouldCallPublisherWithCorrectEventName()
    {
        // Arrange
        var mockPublisher = new Mock<ICapPublisher>();
        var eventBus = new TestableCapDistributedEventBus(mockPublisher.Object);
        var eventData = new TestEvent { Id = 1, Name = "test" };

        // Act
        await eventBus.PublishAsync(eventData);

        // Assert
        mockPublisher.Verify(
            p => p.PublishAsync(
                TestEvent.EventName,
                eventData,
                (string?)null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// 发布事件时应将 CancellationToken 传递给 ICapPublisher
    /// </summary>
    [Fact]
    public async Task PublishAsync_ShouldPassCancellationToken()
    {
        // Arrange
        var mockPublisher = new Mock<ICapPublisher>();
        var eventBus = new TestableCapDistributedEventBus(mockPublisher.Object);
        var eventData = new TestEvent { Id = 1, Name = "test" };
        var cancellationToken = new CancellationToken(true);

        // Act
        await eventBus.PublishAsync(eventData, cancellationToken);

        // Assert
        mockPublisher.Verify(
            p => p.PublishAsync(
                TestEvent.EventName,
                eventData,
                (string?)null,
                cancellationToken),
            Times.Once);
    }

    /// <summary>
    /// 发布没有 EventNameAttribute 的事件时应抛出 InvalidOperationException
    /// </summary>
    [Fact]
    public async Task PublishAsync_WithEventMissingEventNameAttribute_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var mockPublisher = new Mock<ICapPublisher>();
        var eventBus = new TestableCapDistributedEventBus(mockPublisher.Object);
        var eventData = new EventWithoutAttribute();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => eventBus.PublishAsync(eventData));
        Assert.Contains("Event name cannot be empty", exception.Message);
    }

    /// <summary>
    /// 发布事件时应将正确的事件数据传递给 ICapPublisher
    /// </summary>
    [Fact]
    public async Task PublishAsync_ShouldPassCorrectEventData()
    {
        // Arrange
        var mockPublisher = new Mock<ICapPublisher>();
        var eventBus = new TestableCapDistributedEventBus(mockPublisher.Object);
        var eventData = new TestEvent { Id = 42, Name = "specific-data" };

        // Act
        await eventBus.PublishAsync(eventData);

        // Assert
        mockPublisher.Verify(
            p => p.PublishAsync(
                It.IsAny<string>(),
                eventData,
                (string?)null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region PublishDelayAsync

    /// <summary>
    /// 延迟发布带有有效 EventNameAttribute 的事件时，应使用正确的事件名称调用 ICapPublisher.PublishAsync
    /// </summary>
    [Fact]
    public async Task PublishDelayAsync_WithValidEvent_ShouldCallPublisherWithCorrectEventName()
    {
        // Arrange
        var mockPublisher = new Mock<ICapPublisher>();
        var eventBus = new TestableCapDistributedEventBus(mockPublisher.Object);
        var eventData = new TestEvent { Id = 1, Name = "test" };
        var delayTime = TimeSpan.FromSeconds(10);

        // Act
        await eventBus.PublishDelayAsync(delayTime, eventData);

        // Assert
        mockPublisher.Verify(
            p => p.PublishAsync(
                TestEvent.EventName,
                eventData,
                (string?)null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// 延迟发布事件时应将 CancellationToken 传递给 ICapPublisher
    /// </summary>
    [Fact]
    public async Task PublishDelayAsync_ShouldPassCancellationToken()
    {
        // Arrange
        var mockPublisher = new Mock<ICapPublisher>();
        var eventBus = new TestableCapDistributedEventBus(mockPublisher.Object);
        var eventData = new TestEvent { Id = 1, Name = "test" };
        var delayTime = TimeSpan.FromMinutes(5);
        var cancellationToken = new CancellationToken(true);

        // Act
        await eventBus.PublishDelayAsync(delayTime, eventData, cancellationToken);

        // Assert
        mockPublisher.Verify(
            p => p.PublishAsync(
                TestEvent.EventName,
                eventData,
                (string?)null,
                cancellationToken),
            Times.Once);
    }

    /// <summary>
    /// 延迟发布没有 EventNameAttribute 的事件时应抛出 InvalidOperationException
    /// </summary>
    [Fact]
    public async Task PublishDelayAsync_WithEventMissingEventNameAttribute_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var mockPublisher = new Mock<ICapPublisher>();
        var eventBus = new TestableCapDistributedEventBus(mockPublisher.Object);
        var eventData = new EventWithoutAttribute();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => eventBus.PublishDelayAsync(TimeSpan.FromSeconds(1), eventData));
        Assert.Contains("Event name cannot be empty", exception.Message);
    }

    /// <summary>
    /// PublishDelayAsync 在事件名有效时应调用 ICapPublisher
    /// </summary>
    [Fact]
    public async Task PublishDelayAsync_ShouldInvokePublisher()
    {
        // Arrange
        var mockPublisher = new Mock<ICapPublisher>();
        var eventBus = new TestableCapDistributedEventBus(mockPublisher.Object);
        var eventData = new TestEvent { Id = 1, Name = "test" };

        // Act
        await eventBus.PublishDelayAsync(TimeSpan.Zero, eventData);

        // Assert
        mockPublisher.Verify(
            p => p.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<TestEvent>(),
                (string?)null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetEventName

    /// <summary>
    /// GetEventName 对于带有有效 EventNameAttribute 的事件应返回正确的事件名称
    /// </summary>
    [Fact]
    public void GetEventName_WithValidAttribute_ShouldReturnCorrectName()
    {
        // Arrange
        var mockPublisher = new Mock<ICapPublisher>();
        var eventBus = new TestableCapDistributedEventBus(mockPublisher.Object);

        // Act
        var eventName = eventBus.ExposeGetEventName(new TestEvent());

        // Assert
        Assert.Equal(TestEvent.EventName, eventName);
    }

    /// <summary>
    /// GetEventName 对于没有 EventNameAttribute 的事件应抛出 InvalidOperationException
    /// </summary>
    [Fact]
    public void GetEventName_WithoutAttribute_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var mockPublisher = new Mock<ICapPublisher>();
        var eventBus = new TestableCapDistributedEventBus(mockPublisher.Object);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => eventBus.ExposeGetEventName(new EventWithoutAttribute()));
        Assert.Contains("Event name cannot be empty", exception.Message);
    }

    #endregion

    #region Constructor

    /// <summary>
    /// CapDistributedEventBus 应实现 IDistributedEventBus 接口
    /// </summary>
    [Fact]
    public void CapDistributedEventBus_ShouldImplementIDistributedEventBus()
    {
        var mockPublisher = new Mock<ICapPublisher>();
        var eventBus = new CapDistributedEventBus(mockPublisher.Object);

        Assert.IsAssignableFrom<Abstractions.Distributed.IDistributedEventBus>(eventBus);
    }

    #endregion

    #region Test Helpers

    /// <summary>
    /// 用于测试的可派生类，暴露 protected 方法
    /// </summary>
    private class TestableCapDistributedEventBus : CapDistributedEventBus
    {
        public TestableCapDistributedEventBus(ICapPublisher publisher) : base(publisher)
        {
        }

        public string ExposeGetEventName<TEvent>(TEvent eventData)
        {
            return GetEventName(eventData);
        }
    }

    #endregion
}

#region Test Events

[EventName(TestEvent.EventName)]
public class TestEvent
{
    public const string EventName = "test.event.created";
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class EventWithoutAttribute
{
    public int Id { get; set; }
}

#endregion
