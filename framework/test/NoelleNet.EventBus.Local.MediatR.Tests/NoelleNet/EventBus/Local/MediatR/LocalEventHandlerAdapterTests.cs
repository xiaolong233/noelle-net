using MediatR;
using Moq;
using NoelleNet.EventBus.Abstractions.Local;

namespace NoelleNet.EventBus.Local.MediatR;

/// <summary>
/// <see cref="LocalEventHandlerAdapter"/> 的单元测试
/// </summary>
public class LocalEventHandlerAdapterTests
{
    #region Constructor

    /// <summary>
    /// 传入 null 的 IServiceProvider 时应抛出 ArgumentNullException
    /// </summary>
    [Fact]
    public void Constructor_WithNullServiceProvider_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new LocalEventHandlerAdapter(null!));
    }

    /// <summary>
    /// 传入有效的 IServiceProvider 时应成功创建实例
    /// </summary>
    [Fact]
    public void Constructor_WithValidServiceProvider_ShouldCreateInstance()
    {
        var mockServiceProvider = new Mock<IServiceProvider>();
        var adapter = new LocalEventHandlerAdapter(mockServiceProvider.Object);
        Assert.NotNull(adapter);
    }

    #endregion

    #region Handle

    /// <summary>
    /// 当存在匹配的事件处理程序时，应调用 HandleAsync
    /// </summary>
    [Fact]
    public async Task Handle_WithMatchingHandler_ShouldInvokeHandler()
    {
        // Arrange
        var handler = new TestHandler();
        var mockServiceProvider = CreateServiceProvider(typeof(TestHandlerEvent), new[] { handler });
        var adapter = new LocalEventHandlerAdapter(mockServiceProvider.Object);
        var notification = new LocalEventAdapter(new TestHandlerEvent { Data = "test-data" });

        // Act
        await adapter.Handle(notification, CancellationToken.None);

        // Assert
        Assert.True(handler.WasInvoked);
        Assert.Equal("test-data", handler.ReceivedEvent!.Data);
    }

    /// <summary>
    /// 当存在多个匹配的事件处理程序时，应依次调用所有处理程序
    /// </summary>
    [Fact]
    public async Task Handle_WithMultipleHandlers_ShouldInvokeAllHandlers()
    {
        // Arrange
        var handler1 = new TestHandler();
        var handler2 = new TestHandler();
        var handlers = new ILocalEventHandler<TestHandlerEvent>[] { handler1, handler2 };
        var mockServiceProvider = CreateServiceProvider(typeof(TestHandlerEvent), handlers);
        var adapter = new LocalEventHandlerAdapter(mockServiceProvider.Object);
        var notification = new LocalEventAdapter(new TestHandlerEvent { Data = "multi" });

        // Act
        await adapter.Handle(notification, CancellationToken.None);

        // Assert
        Assert.True(handler1.WasInvoked);
        Assert.True(handler2.WasInvoked);
    }

    /// <summary>
    /// 当没有匹配的事件处理程序时，Handle 方法不应抛出异常
    /// </summary>
    [Fact]
    public async Task Handle_WithNoHandlers_ShouldNotThrow()
    {
        // Arrange
        var mockServiceProvider = CreateServiceProvider(typeof(TestHandlerEvent), Array.Empty<ILocalEventHandler<TestHandlerEvent>>());
        var adapter = new LocalEventHandlerAdapter(mockServiceProvider.Object);
        var notification = new LocalEventAdapter(new TestHandlerEvent { Data = "no-handlers" });

        // Act & Assert
        var exception = await Record.ExceptionAsync(
            () => adapter.Handle(notification, CancellationToken.None));
        Assert.Null(exception);
    }

    /// <summary>
    /// Handle 方法应将 CancellationToken 传递给事件处理程序
    /// </summary>
    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToHandler()
    {
        // Arrange
        var handler = new TestHandler();
        var mockServiceProvider = CreateServiceProvider(typeof(TestHandlerEvent), new[] { handler });
        var adapter = new LocalEventHandlerAdapter(mockServiceProvider.Object);
        var notification = new LocalEventAdapter(new TestHandlerEvent { Data = "ct-test" });
        var cancellationToken = new CancellationToken(true);

        // Act
        await adapter.Handle(notification, cancellationToken);

        // Assert
        Assert.True(handler.ReceivedCancellationToken.IsCancellationRequested);
    }

    /// <summary>
    /// 当 DI 返回包含 null 元素的数组时，应跳过 null 项继续处理
    /// </summary>
    [Fact]
    public async Task Handle_ShouldSkipNullHandlers()
    {
        // Arrange
        var handler = new TestHandler();
        ILocalEventHandler<TestHandlerEvent>[] handlers = [null!, handler];
        var mockServiceProvider = CreateServiceProvider(typeof(TestHandlerEvent), handlers);
        var adapter = new LocalEventHandlerAdapter(mockServiceProvider.Object);
        var notification = new LocalEventAdapter(new TestHandlerEvent { Data = "skip-null" });

        // Act
        await adapter.Handle(notification, CancellationToken.None);

        // Assert
        Assert.True(handler.WasInvoked);
    }

    /// <summary>
    /// 对不同事件类型应分别调用对应类型的处理程序
    /// </summary>
    [Fact]
    public async Task Handle_WithDifferentEventTypes_ShouldInvokeCorrectHandlers()
    {
        // Arrange
        var handler1 = new TestHandler();
        var handler2 = new AnotherTestHandler();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<ILocalEventHandler<TestHandlerEvent>>)))
            .Returns(new ILocalEventHandler<TestHandlerEvent>[] { handler1 });
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<ILocalEventHandler<AnotherTestHandlerEvent>>)))
            .Returns(new ILocalEventHandler<AnotherTestHandlerEvent>[] { handler2 });

        var adapter = new LocalEventHandlerAdapter(mockServiceProvider.Object);
        var notification1 = new LocalEventAdapter(new TestHandlerEvent { Data = "type1" });
        var notification2 = new LocalEventAdapter(new AnotherTestHandlerEvent { Id = 100 });

        // Act
        await adapter.Handle(notification1, CancellationToken.None);
        await adapter.Handle(notification2, CancellationToken.None);

        // Assert
        Assert.True(handler1.WasInvoked);
        Assert.Equal("type1", handler1.ReceivedEvent!.Data);
        Assert.True(handler2.WasInvoked);
        Assert.Equal(100, handler2.ReceivedEvent!.Id);
    }

    /// <summary>
    /// 同一事件类型的 Handle 委托应被缓存复用
    /// </summary>
    [Fact]
    public async Task Handle_SameEventType_ShouldUseCachedDelegate()
    {
        // Arrange
        var handler1 = new TestHandler();
        var handler2 = new TestHandler();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var serviceType = typeof(IEnumerable<ILocalEventHandler<TestHandlerEvent>>);
        mockServiceProvider
            .SetupSequence(sp => sp.GetService(serviceType))
            .Returns(new ILocalEventHandler<TestHandlerEvent>[] { handler1 })
            .Returns(new ILocalEventHandler<TestHandlerEvent>[] { handler2 });

        var adapter = new LocalEventHandlerAdapter(mockServiceProvider.Object);

        // Act — 第一次调用会编译并缓存委托
        var notification1 = new LocalEventAdapter(new TestHandlerEvent { Data = "first" });
        await adapter.Handle(notification1, CancellationToken.None);

        // 第二次调用应使用缓存的委托
        var notification2 = new LocalEventAdapter(new TestHandlerEvent { Data = "second" });
        await adapter.Handle(notification2, CancellationToken.None);

        // Assert — 两次调用的服务类型相同，GetService 被调用两次（获取 handler 列表），
        // 但委托只应编译一次
        Assert.True(handler1.WasInvoked);
        Assert.Equal("first", handler1.ReceivedEvent!.Data);
        Assert.True(handler2.WasInvoked);
        Assert.Equal("second", handler2.ReceivedEvent!.Data);
    }

    #endregion

    #region Interface Implementation

    /// <summary>
    /// LocalEventHandlerAdapter 应实现 INotificationHandler&lt;LocalEventAdapter&gt; 接口
    /// </summary>
    [Fact]
    public void ShouldImplementINotificationHandler()
    {
        var mockServiceProvider = new Mock<IServiceProvider>();
        var adapter = new LocalEventHandlerAdapter(mockServiceProvider.Object);
        Assert.IsAssignableFrom<INotificationHandler<LocalEventAdapter>>(adapter);
    }

    #endregion

    #region Helpers

    private static Mock<IServiceProvider> CreateServiceProvider<TEvent>(
        Type eventType,
        IEnumerable<ILocalEventHandler<TEvent>> handlers)
    {
        var serviceType = typeof(IEnumerable<>).MakeGenericType(
            typeof(ILocalEventHandler<>).MakeGenericType(eventType));
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(serviceType))
            .Returns(handlers);
        return mockServiceProvider;
    }

    #endregion
}

#region Test Events and Handlers

public class TestHandlerEvent
{
    public string Data { get; set; } = string.Empty;
}

public class TestHandler : ILocalEventHandler<TestHandlerEvent>
{
    public bool WasInvoked { get; private set; }
    public TestHandlerEvent? ReceivedEvent { get; private set; }
    public CancellationToken ReceivedCancellationToken { get; private set; }

    public Task HandleAsync(TestHandlerEvent eventData, CancellationToken cancellationToken = default)
    {
        WasInvoked = true;
        ReceivedEvent = eventData;
        ReceivedCancellationToken = cancellationToken;
        return Task.CompletedTask;
    }
}

public class AnotherTestHandlerEvent
{
    public int Id { get; set; }
}

public class AnotherTestHandler : ILocalEventHandler<AnotherTestHandlerEvent>
{
    public bool WasInvoked { get; private set; }
    public AnotherTestHandlerEvent? ReceivedEvent { get; private set; }

    public Task HandleAsync(AnotherTestHandlerEvent eventData, CancellationToken cancellationToken = default)
    {
        WasInvoked = true;
        ReceivedEvent = eventData;
        return Task.CompletedTask;
    }
}

#endregion
