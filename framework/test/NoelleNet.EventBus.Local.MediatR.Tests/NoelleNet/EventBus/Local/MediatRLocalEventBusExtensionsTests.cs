using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NoelleNet.EventBus.Abstractions.Local;
using NoelleNet.EventBus.Local.MediatR;

namespace NoelleNet.EventBus.Local;

/// <summary>
/// <see cref="MediatRLocalEventBusExtensions"/> 的单元测试
/// </summary>
public class MediatRLocalEventBusExtensionsTests
{
    #region UseMediatR

    /// <summary>
    /// 传入 null 的 configuration 时应抛出 ArgumentNullException
    /// </summary>
    [Fact]
    public void UseMediatR_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => MediatRLocalEventBusExtensions.UseMediatR(null!, _ => { }));
    }

    /// <summary>
    /// 传入 null 的 configure 时应抛出 ArgumentNullException
    /// </summary>
    [Fact]
    public void UseMediatR_WithNullConfigure_ShouldThrowArgumentNullException()
    {
        var services = new ServiceCollection();
        var configuration = new LocalEventBusConfiguration(services);

        Assert.Throws<ArgumentNullException>(
            () => MediatRLocalEventBusExtensions.UseMediatR(configuration, null!));
    }

    /// <summary>
    /// UseMediatR 应将 MediatRLocalEventBus 注册为 ILocalEventBus 的 Scoped 实现
    /// </summary>
    [Fact]
    public void UseMediatR_ShouldRegisterMediatRLocalEventBusAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new LocalEventBusConfiguration(services);

        // Act
        configuration.UseMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(MediatRLocalEventBusExtensionsTests).Assembly));

        // Assert
        var descriptor = services.FirstOrDefault(s =>
            s.ServiceType == typeof(ILocalEventBus) &&
            s.ImplementationType == typeof(MediatRLocalEventBus));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    /// <summary>
    /// UseMediatR 应将 LocalEventHandlerAdapter 注册为 INotificationHandler&lt;LocalEventAdapter&gt; 的 Transient 实现
    /// </summary>
    [Fact]
    public void UseMediatR_ShouldRegisterLocalEventHandlerAdapterAsTransient()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new LocalEventBusConfiguration(services);

        // Act
        configuration.UseMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(MediatRLocalEventBusExtensionsTests).Assembly));

        // Assert
        var descriptor = services.FirstOrDefault(s =>
            s.Lifetime == ServiceLifetime.Transient &&
            s.ServiceType == typeof(INotificationHandler<LocalEventAdapter>) &&
            s.ImplementationType == typeof(LocalEventHandlerAdapter));
        Assert.NotNull(descriptor);
    }

    /// <summary>
    /// UseMediatR 应返回相同的 configuration 实例以支持链式调用
    /// </summary>
    [Fact]
    public void UseMediatR_ShouldReturnSameConfigurationForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new LocalEventBusConfiguration(services);

        // Act
        var result = configuration.UseMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(MediatRLocalEventBusExtensionsTests).Assembly));

        // Assert
        Assert.Same(configuration, result);
    }

    /// <summary>
    /// UseMediatR 应调用传入的 configure 委托
    /// </summary>
    [Fact]
    public void UseMediatR_ShouldCallConfigureAction()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new LocalEventBusConfiguration(services);
        bool configureInvoked = false;

        // Act
        configuration.UseMediatR(cfg =>
        {
            configureInvoked = true;
            cfg.RegisterServicesFromAssembly(typeof(MediatRLocalEventBusExtensionsTests).Assembly);
        });

        // Assert
        Assert.True(configureInvoked);
    }

    /// <summary>
    /// UseMediatR 应向 configure 委托传入有效的 MediatRServiceConfiguration 实例
    /// </summary>
    [Fact]
    public void UseMediatR_ShouldPassValidMediatRServiceConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new LocalEventBusConfiguration(services);
        MediatRServiceConfiguration? receivedConfig = null;

        // Act
        configuration.UseMediatR(cfg =>
        {
            receivedConfig = cfg;
            cfg.RegisterServicesFromAssembly(typeof(MediatRLocalEventBusExtensionsTests).Assembly);
        });

        // Assert
        Assert.NotNull(receivedConfig);
    }

    /// <summary>
    /// 多次调用 UseMediatR 不应抛出异常
    /// </summary>
    [Fact]
    public void UseMediatR_CalledMultipleTimes_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new LocalEventBusConfiguration(services);

        // Act & Assert
        configuration.UseMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(MediatRLocalEventBusExtensionsTests).Assembly));
        var exception = Record.Exception(() =>
            configuration.UseMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(MediatRLocalEventBusExtensionsTests).Assembly)));
        Assert.Null(exception);
    }

    /// <summary>
    /// UseMediatR 注册后 services 集合不应为空
    /// </summary>
    [Fact]
    public void UseMediatR_ShouldAddServicesToCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new LocalEventBusConfiguration(services);

        // Act
        configuration.UseMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(MediatRLocalEventBusExtensionsTests).Assembly));

        // Assert
        Assert.NotEmpty(services);
    }

    #endregion
}
