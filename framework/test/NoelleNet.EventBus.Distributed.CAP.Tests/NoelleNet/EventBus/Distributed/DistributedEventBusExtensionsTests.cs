using Microsoft.Extensions.DependencyInjection;
using NoelleNet.EventBus.Abstractions.Distributed;

namespace NoelleNet.EventBus.Distributed;

/// <summary>
/// <see cref="DistributedEventBusExtensions"/> 的单元测试
/// </summary>
public class DistributedEventBusExtensionsTests
{
    /// <summary>
    /// 传入包含分布式事件处理器的程序集时，应注册对应的 IDistributedEventHandler&lt;&gt; 服务
    /// </summary>
    [Fact]
    public void AddDistributedEventBus_WithAssemblyContainingHandler_ShouldRegisterHandler()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDistributedEventBus(cfg => cfg.RegisterServicesFromAssembly(typeof(TestDistributedEventHandler).Assembly));

        // Assert
        var descriptor = services.FirstOrDefault(s =>
            s.ServiceType == typeof(IDistributedEventHandler<TestDistributedEvent>) &&
            s.ImplementationType == typeof(TestDistributedEventHandler));

        Assert.NotNull(descriptor);
    }

    /// <summary>
    /// 未注册任何程序集时不应抛出异常（空扫描结果）
    /// </summary>
    [Fact]
    public void AddDistributedEventBus_WithNoAssemblies_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var exception = Record.Exception(() => services.AddDistributedEventBus(_ => { }));
        Assert.Null(exception);
    }

    /// <summary>
    /// 抽象处理器类型不应被注册
    /// </summary>
    [Fact]
    public void AddDistributedEventBus_ShouldNotRegisterAbstractHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDistributedEventBus(cfg => cfg.RegisterServicesFromAssembly(typeof(AbstractTestDistributedEventHandler).Assembly));

        // Assert
        var descriptor = services.FirstOrDefault(s =>
            s.ServiceType == typeof(IDistributedEventHandler<TestDistributedEvent>) &&
            s.ImplementationType == typeof(AbstractTestDistributedEventHandler));

        Assert.Null(descriptor);
    }

    #region Test Types

    public class TestDistributedEvent;

    public class TestDistributedEventHandler : IDistributedEventHandler<TestDistributedEvent>
    {
        public Task HandleAsync(TestDistributedEvent eventData, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    public abstract class AbstractTestDistributedEventHandler : IDistributedEventHandler<TestDistributedEvent>
    {
        public abstract Task HandleAsync(TestDistributedEvent eventData, CancellationToken cancellationToken = default);
    }

    #endregion
}
