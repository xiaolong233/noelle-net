using DotNetCore.CAP;
using DotNetCore.CAP.Internal;
using Microsoft.Extensions.DependencyInjection;
using NoelleNet.EventBus.Abstractions.Distributed;

namespace NoelleNet.EventBus.Distributed;

/// <summary>
/// <see cref="CapDistributedEventBusExtensions"/> 的单元测试
/// </summary>
public class CapDistributedEventBusExtensionsTests
{
    #region UseCap

    /// <summary>
    /// UseCap 应将 CapDistributedEventBus 注册为 IDistributedEventBus 的 Scoped 服务
    /// </summary>
    [Fact]
    public void UseCap_ShouldRegisterCapDistributedEventBusAsScopedService()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new DistributedEventBusConfiguration(services);

        // Act
        configuration.UseCap(_ => { });

        // Assert
        var descriptor = services.FirstOrDefault(s =>
            s.ServiceType == typeof(IDistributedEventBus) &&
            s.ImplementationType == typeof(CAP.CapDistributedEventBus));

        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    /// <summary>
    /// UseCap 应将 NoelleConsumerServiceSelector 注册为 IConsumerServiceSelector 的 Singleton 服务
    /// </summary>
    [Fact]
    public void UseCap_ShouldRegisterNoelleConsumerServiceSelectorAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new DistributedEventBusConfiguration(services);

        // Act
        configuration.UseCap(_ => { });

        // Assert
        var descriptor = services.FirstOrDefault(s =>
            s.ServiceType == typeof(IConsumerServiceSelector) &&
            s.ImplementationType == typeof(CAP.NoelleConsumerServiceSelector));

        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    /// <summary>
    /// UseCap 应调用 IServiceCollection.AddCap 注册 CAP 服务
    /// </summary>
    [Fact]
    public void UseCap_ShouldCallAddCapWithSetupAction()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new DistributedEventBusConfiguration(services);
        bool setupActionInvoked = false;
        Action<CapOptions> setupAction = _ => setupActionInvoked = true;

        // Act
        configuration.UseCap(setupAction);

        // Assert
        Assert.True(setupActionInvoked);
    }

    /// <summary>
    /// UseCap 应返回相同的 DistributedEventBusConfiguration 实例以支持链式调用
    /// </summary>
    [Fact]
    public void UseCap_ShouldReturnSameConfigurationForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new DistributedEventBusConfiguration(services);

        // Act
        var result = configuration.UseCap(_ => { });

        // Assert
        Assert.Same(configuration, result);
    }

    /// <summary>
    /// UseCap 多次调用应能正常工作（Scoped 服务会覆盖注册）
    /// </summary>
    [Fact]
    public void UseCap_CalledMultipleTimes_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new DistributedEventBusConfiguration(services);

        // Act & Assert - 确保多次调用不抛出异常
        configuration.UseCap(_ => { });
        var exception = Record.Exception(() => configuration.UseCap(_ => { }));
        Assert.Null(exception);
    }

    /// <summary>
    /// UseCap 注册后，ServiceCollection 应包含多个服务描述符
    /// </summary>
    [Fact]
    public void UseCap_ShouldAddCapRelatedServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new DistributedEventBusConfiguration(services);

        // Act
        configuration.UseCap(_ => { });

        // Assert
        Assert.NotEmpty(services);
    }

    /// <summary>
    /// UseCap 调用 AddCap 时会传入 CapOptions，可通过 setupAction 验证
    /// </summary>
    [Fact]
    public void UseCap_ShouldPassCapOptionsToSetupAction()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new DistributedEventBusConfiguration(services);
        CapOptions? receivedOptions = null;

        // Act
        configuration.UseCap(options => receivedOptions = options);

        // Assert
        Assert.NotNull(receivedOptions);
    }

    #endregion
}
