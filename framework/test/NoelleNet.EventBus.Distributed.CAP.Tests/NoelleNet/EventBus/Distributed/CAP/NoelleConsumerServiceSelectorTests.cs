using DotNetCore.CAP;
using DotNetCore.CAP.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NoelleNet.EventBus.Abstractions;
using NoelleNet.EventBus.Abstractions.Distributed;
using NoelleNet.EventBus.Distributed;
using System.Reflection;

namespace NoelleNet.EventBus.Distributed.CAP;

/// <summary>
/// <see cref="NoelleConsumerServiceSelector"/> 的契约测试：
/// 由事件类型（EventNameAttribute）驱动的 CAP 消费者描述符构造
/// </summary>
public class NoelleConsumerServiceSelectorTests
{
    /// <summary>
    /// 描述符应包含 Topic 名、HandleAsync 方法、实现类型、服务类型与参数
    /// </summary>
    [Fact]
    public void GetHandlerDescription_WithValidTypes_ShouldReturnCompleteDescriptor()
    {
        var selector = CreateSelector();

        var descriptor = selector.ExposeGetHandlerDescription(
            typeof(CreateOrderEvent), typeof(CreateOrderEventHandler)).Single();

        Assert.Equal(CreateOrderEvent.EventName, descriptor.Attribute.Name);
        Assert.Equal(nameof(IDistributedEventHandler<object>.HandleAsync), descriptor.MethodInfo.Name);
        Assert.Equal(typeof(CreateOrderEventHandler), descriptor.ImplTypeInfo.AsType());
        Assert.Equal(typeof(IDistributedEventHandler<>), descriptor.ServiceTypeInfo.GetGenericTypeDefinition());
        Assert.Contains(descriptor.Parameters, p => p.ParameterType == typeof(CreateOrderEvent));
    }

    /// <summary>
    /// 事件缺少 EventNameAttribute 时应抛出 InvalidOperationException
    /// </summary>
    [Fact]
    public void GetHandlerDescription_WithEventMissingEventNameAttribute_ShouldThrow()
    {
        var selector = CreateSelector();

        var exception = Assert.Throws<InvalidOperationException>(
            () => selector.ExposeGetHandlerDescription(
                typeof(EventMissingAttribute), typeof(EventHandlerForEventMissingAttribute)).ToList());

        Assert.Contains("Event name cannot be empty", exception.Message);
    }

    /// <summary>
    /// 事件设置了 Group 时使用自定义分组；未设置时由 CAP 默认分组填充
    /// </summary>
    [Fact]
    public void GetHandlerDescription_Group_ShouldUseCustomOrDefaultGroup()
    {
        var selector = CreateSelector();

        var withGroup = selector.ExposeGetHandlerDescription(
            typeof(EventWithGroup), typeof(EventHandlerForEventWithGroup)).Single();
        Assert.StartsWith(EventWithGroup.GroupName, withGroup.Attribute.Group);

        var withoutGroup = selector.ExposeGetHandlerDescription(
            typeof(CreateOrderEvent), typeof(CreateOrderEventHandler)).Single();
        Assert.False(string.IsNullOrEmpty(withoutGroup.Attribute.Group));
    }

    /// <summary>
    /// CancellationToken 参数应被标记为 FromCap
    /// </summary>
    [Fact]
    public void GetHandlerDescription_CancellationTokenParameter_ShouldBeMarkedFromCap()
    {
        var selector = CreateSelector();

        var descriptor = selector.ExposeGetHandlerDescription(
            typeof(CreateOrderEvent), typeof(HandlerWithCancellationToken)).Single();

        Assert.Contains(descriptor.Parameters, p => p.ParameterType == typeof(CancellationToken) && p.IsFromCap);
    }

    #region Helpers

    private static TestableNoelleConsumerServiceSelector CreateSelector()
    {
        var options = Options.Create(new NoelleDistributedEventBusOptions());
        var capOptions = Options.Create(new CapOptions());
        var mockLogger = new Mock<ILogger<ConsumerServiceSelector>>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IOptions<CapOptions>)))
            .Returns(capOptions);
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(ILogger<ConsumerServiceSelector>)))
            .Returns(mockLogger.Object);

        return new TestableNoelleConsumerServiceSelector(mockServiceProvider.Object, options);
    }

    private class TestableNoelleConsumerServiceSelector : NoelleConsumerServiceSelector
    {
        public TestableNoelleConsumerServiceSelector(
            IServiceProvider serviceProvider,
            IOptions<NoelleDistributedEventBusOptions> options)
            : base(serviceProvider, options)
        {
        }

        public IEnumerable<ConsumerExecutorDescriptor> ExposeGetHandlerDescription(
            Type eventType, Type handlerType)
        {
            return GetHandlerDescription(eventType, handlerType);
        }
    }

    #endregion
}

#region Test Events and Handlers

[EventName(CreateOrderEvent.EventName)]
public class CreateOrderEvent
{
    public const string EventName = "order.created";
    public int OrderId { get; set; }
}

public class CreateOrderEventHandler : IDistributedEventHandler<CreateOrderEvent>
{
    public Task HandleAsync(CreateOrderEvent eventData, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

[EventName(EventWithGroup.EventName, Group = EventWithGroup.GroupName)]
public class EventWithGroup
{
    public const string EventName = "group.event";
    public const string GroupName = "my-group";
}

public class EventHandlerForEventWithGroup : IDistributedEventHandler<EventWithGroup>
{
    public Task HandleAsync(EventWithGroup eventData, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

public class HandlerWithCancellationToken : IDistributedEventHandler<CreateOrderEvent>
{
    public Task HandleAsync(CreateOrderEvent eventData, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

public class EventMissingAttribute
{
    public int Id { get; set; }
}

public class EventHandlerForEventMissingAttribute : IDistributedEventHandler<EventMissingAttribute>
{
    public Task HandleAsync(EventMissingAttribute eventData, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

#endregion
