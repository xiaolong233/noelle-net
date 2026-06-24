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
/// <see cref="NoelleConsumerServiceSelector"/> 的单元测试
/// </summary>
public class NoelleConsumerServiceSelectorTests
{
    #region GetHandlerDescription

    /// <summary>
    /// 给定有效的事件类型和处理程序类型，GetHandlerDescription 应返回包含正确 Topic 属性名称的 ConsumerExecutorDescriptor
    /// </summary>
    [Fact]
    public void GetHandlerDescription_WithValidTypes_ShouldReturnDescriptorWithCorrectTopicName()
    {
        // Arrange
        var selector = CreateSelector();

        // Act
        var descriptors = selector.ExposeGetHandlerDescription(
            typeof(CreateOrderEvent), typeof(CreateOrderEventHandler));

        // Assert
        var descriptorList = descriptors.ToList();
        Assert.Single(descriptorList);
        var descriptor = descriptorList[0];
        Assert.NotNull(descriptor.Attribute);
        Assert.Equal(CreateOrderEvent.EventName, descriptor.Attribute.Name);
    }

    /// <summary>
    /// GetHandlerDescription 返回的描述符应包含正确的 MethodInfo（HandleAsync 方法）
    /// </summary>
    [Fact]
    public void GetHandlerDescription_ShouldReturnDescriptorWithCorrectMethodInfo()
    {
        // Arrange
        var selector = CreateSelector();

        // Act
        var descriptors = selector.ExposeGetHandlerDescription(
            typeof(CreateOrderEvent), typeof(CreateOrderEventHandler));

        // Assert
        var descriptorList = descriptors.ToList();
        Assert.Single(descriptorList);
        var descriptor = descriptorList[0];
        Assert.NotNull(descriptor.MethodInfo);
        Assert.Equal(nameof(IDistributedEventHandler<object>.HandleAsync),
            descriptor.MethodInfo.Name);
    }

    /// <summary>
    /// GetHandlerDescription 返回的描述符应包含正确的处理程序类型信息
    /// </summary>
    [Fact]
    public void GetHandlerDescription_ShouldReturnDescriptorWithCorrectHandlerType()
    {
        // Arrange
        var selector = CreateSelector();

        // Act
        var descriptors = selector.ExposeGetHandlerDescription(
            typeof(CreateOrderEvent), typeof(CreateOrderEventHandler));

        // Assert
        var descriptorList = descriptors.ToList();
        Assert.Single(descriptorList);
        var descriptor = descriptorList[0];
        Assert.Equal(typeof(CreateOrderEventHandler), descriptor.ImplTypeInfo.AsType());
    }

    /// <summary>
    /// GetHandlerDescription 返回的描述符应包含正确的服务类型信息（IDistributedEventHandler&lt;TEvent&gt;）
    /// </summary>
    [Fact]
    public void GetHandlerDescription_ShouldReturnDescriptorWithCorrectServiceType()
    {
        // Arrange
        var selector = CreateSelector();

        // Act
        var descriptors = selector.ExposeGetHandlerDescription(
            typeof(CreateOrderEvent), typeof(CreateOrderEventHandler));

        // Assert
        var descriptorList = descriptors.ToList();
        Assert.Single(descriptorList);
        var descriptor = descriptorList[0];
        Assert.NotNull(descriptor.ServiceTypeInfo);
        Assert.True(descriptor.ServiceTypeInfo.IsGenericType);
        Assert.Equal(typeof(IDistributedEventHandler<>),
            descriptor.ServiceTypeInfo.GetGenericTypeDefinition());
    }

    /// <summary>
    /// GetHandlerDescription 返回的描述符应包含方法参数
    /// </summary>
    [Fact]
    public void GetHandlerDescription_ShouldReturnDescriptorWithParameters()
    {
        // Arrange
        var selector = CreateSelector();

        // Act
        var descriptors = selector.ExposeGetHandlerDescription(
            typeof(CreateOrderEvent), typeof(CreateOrderEventHandler));

        // Assert
        var descriptorList = descriptors.ToList();
        Assert.Single(descriptorList);
        Assert.NotEmpty(descriptorList[0].Parameters);
        Assert.Contains(descriptorList[0].Parameters,
            p => p.ParameterType == typeof(CreateOrderEvent));
    }

    /// <summary>
    /// 当事件类型缺少 EventNameAttribute 时应抛出 InvalidOperationException
    /// </summary>
    [Fact]
    public void GetHandlerDescription_WithEventMissingEventNameAttribute_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var selector = CreateSelector();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => selector.ExposeGetHandlerDescription(
                typeof(EventMissingAttribute), typeof(EventHandlerForEventMissingAttribute)).ToList());

        Assert.Contains("Event name cannot be empty", exception.Message);
    }

    /// <summary>
    /// 当事件类型设置了 Group 属性时，应正确设置 CapSubscribeAttribute 的 Group
    /// </summary>
    [Fact]
    public void GetHandlerDescription_WithEventGroupSet_ShouldSetGroupOnAttribute()
    {
        // Arrange
        var selector = CreateSelector();

        // Act
        var descriptors = selector.ExposeGetHandlerDescription(
            typeof(EventWithGroup), typeof(EventHandlerForEventWithGroup));

        // Assert
        var descriptor = descriptors.Single();
        Assert.NotNull(descriptor.Attribute.Group);
        Assert.StartsWith(EventWithGroup.GroupName, descriptor.Attribute.Group);
    }

    /// <summary>
    /// 当事件类型未设置 Group 属性时，CAP 的 SetSubscribeAttribute 会用 CapOptions.DefaultGroup 填充默认值
    /// </summary>
    [Fact]
    public void GetHandlerDescription_WithoutEventGroup_ShouldHaveDefaultGroupFromCap()
    {
        // Arrange
        var selector = CreateSelector();

        // Act
        var descriptors = selector.ExposeGetHandlerDescription(
            typeof(CreateOrderEvent), typeof(CreateOrderEventHandler));

        // Assert
        var descriptor = descriptors.Single();
        Assert.False(string.IsNullOrEmpty(descriptor.Attribute.Group));
    }

    /// <summary>
    /// GetHandlerDescription 为需要 CancellationToken 的处理程序应正确识别 FromCap 参数
    /// </summary>
    [Fact]
    public void GetHandlerDescription_WithCancellationTokenParameter_ShouldMarkAsFromCap()
    {
        // Arrange
        var selector = CreateSelector();

        // Act
        var descriptors = selector.ExposeGetHandlerDescription(
            typeof(CreateOrderEvent), typeof(HandlerWithCancellationToken));

        // Assert
        var descriptor = descriptors.Single();
        Assert.Contains(descriptor.Parameters,
            p => p.ParameterType == typeof(CancellationToken) && p.IsFromCap);
    }

    #endregion

    #region Helpers

    /// <summary>
    /// 创建用于测试 GetHandlerDescription 的 NoelleConsumerServiceSelector 实例
    /// 使用 Moq mock IServiceProvider 提供基类构造函数所需的最小依赖
    /// </summary>
    private static TestableNoelleConsumerServiceSelector CreateSelector(
        List<(Type HandlerType, Type EventType)>? pairs = null)
    {
        var optionsObj = new NoelleDistributedEventBusOptions();
        var prop = typeof(NoelleDistributedEventBusOptions)
            .GetProperty(nameof(NoelleDistributedEventBusOptions.HandlerEventTypePairs),
                BindingFlags.Public | BindingFlags.Instance)!;
        prop.SetValue(optionsObj, pairs ?? []);
        var options = Options.Create(optionsObj);

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

    /// <summary>
    /// 用于测试的可派生类，暴露 protected 方法
    /// </summary>
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
