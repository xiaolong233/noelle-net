using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NoelleNet.EventBus.Abstractions.Local;
using NoelleNet.EventBus.Local.MediatR;

namespace NoelleNet.EventBus.Local;

/// <summary>
/// <see cref="MediatRLocalEventBusExtensions"/> 的注册契约测试
/// </summary>
public class MediatRLocalEventBusExtensionsTests
{
    /// <summary>
    /// UseMediatR 应注册 ILocalEventBus（Scoped）与 LocalEventHandlerAdapter（Transient），并执行 configure 委托后返回自身
    /// </summary>
    [Fact]
    public void UseMediatR_ShouldRegisterServicesAndReturnSameConfiguration()
    {
        var services = new ServiceCollection();
        var configuration = new LocalEventBusConfiguration(services);
        bool configureInvoked = false;

        var result = configuration.UseMediatR(cfg =>
        {
            configureInvoked = true;
            cfg.RegisterServicesFromAssembly(typeof(MediatRLocalEventBusExtensionsTests).Assembly);
        });

        Assert.True(configureInvoked);
        Assert.Same(configuration, result);

        var busDescriptor = services.FirstOrDefault(s =>
            s.ServiceType == typeof(ILocalEventBus) &&
            s.ImplementationType == typeof(MediatRLocalEventBus));
        Assert.NotNull(busDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, busDescriptor.Lifetime);

        var adapterDescriptor = services.FirstOrDefault(s =>
            s.Lifetime == ServiceLifetime.Transient &&
            s.ServiceType == typeof(INotificationHandler<LocalEventAdapter>) &&
            s.ImplementationType == typeof(LocalEventHandlerAdapter));
        Assert.NotNull(adapterDescriptor);
    }
}
