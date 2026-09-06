using MediatR;
using Moq;
using NoelleNet.EventBus.Abstractions.Local;

namespace NoelleNet.EventBus.Local.MediatR;

/// <summary>
/// <see cref="LocalEventHandlerAdapter"/> 的行为测试：按事件运行时类型解析处理器并依次调用
/// </summary>
public class LocalEventHandlerAdapterTests
{
    /// <summary>
    /// 单/多处理器应被依次调用（含 CancellationToken 透传与 null 处理器跳过）
    /// </summary>
    [Fact]
    public async Task Handle_ShouldInvokeAllHandlersInOrder()
    {
        var handler1 = new TestHandler();
        var handler2 = new TestHandler();
        ILocalEventHandler<TestHandlerEvent>[] handlers = [handler1, null!, handler2];
        var mockServiceProvider = CreateServiceProvider(handlers);
        var adapter = new LocalEventHandlerAdapter(mockServiceProvider.Object);
        var cancellationToken = new CancellationToken(true);

        await adapter.Handle(new LocalEventAdapter(new TestHandlerEvent { Data = "multi" }), cancellationToken);

        Assert.True(handler1.WasInvoked);
        Assert.Equal("multi", handler1.ReceivedEvent!.Data);
        Assert.True(handler1.ReceivedCancellationToken.IsCancellationRequested);
        Assert.True(handler2.WasInvoked);
    }

    /// <summary>
    /// 无匹配处理器时不应抛出异常
    /// </summary>
    [Fact]
    public async Task Handle_WithNoHandlers_ShouldNotThrow()
    {
        var mockServiceProvider = CreateServiceProvider(Array.Empty<ILocalEventHandler<TestHandlerEvent>>());
        var adapter = new LocalEventHandlerAdapter(mockServiceProvider.Object);

        var exception = await Record.ExceptionAsync(
            () => adapter.Handle(new LocalEventAdapter(new TestHandlerEvent { Data = "no-handlers" }), CancellationToken.None));

        Assert.Null(exception);
    }

    /// <summary>
    /// 不同事件类型应调用对应类型的处理器
    /// </summary>
    [Fact]
    public async Task Handle_WithDifferentEventTypes_ShouldInvokeCorrectHandlers()
    {
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

        await adapter.Handle(new LocalEventAdapter(new TestHandlerEvent { Data = "type1" }), CancellationToken.None);
        await adapter.Handle(new LocalEventAdapter(new AnotherTestHandlerEvent { Id = 100 }), CancellationToken.None);

        Assert.Equal("type1", handler1.ReceivedEvent!.Data);
        Assert.Equal(100, handler2.ReceivedEvent!.Id);
    }

    private static Mock<IServiceProvider> CreateServiceProvider<TEvent>(IEnumerable<ILocalEventHandler<TEvent>> handlers)
    {
        var serviceType = typeof(IEnumerable<>).MakeGenericType(typeof(ILocalEventHandler<>).MakeGenericType(typeof(TEvent)));
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(serviceType))
            .Returns(handlers);
        return mockServiceProvider;
    }
}

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
