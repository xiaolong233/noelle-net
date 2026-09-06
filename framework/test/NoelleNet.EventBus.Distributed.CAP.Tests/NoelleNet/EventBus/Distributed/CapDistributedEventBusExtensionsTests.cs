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
    /// UseCap 应执行 configure 委托并返回相同的配置实例（支持链式调用）
    /// </summary>
    [Fact]
    public void UseCap_ShouldInvokeSetupActionAndReturnSameConfiguration()
    {
        var services = new ServiceCollection();
        var configuration = new DistributedEventBusConfiguration(services);
        bool setupActionInvoked = false;

        var result = configuration.UseCap(_ => setupActionInvoked = true);

        Assert.True(setupActionInvoked);
        Assert.Same(configuration, result);
    }

    /// <summary>
    /// 集成契约：UseCap 先注册自定义选择器、随后 AddCap 以 TryAddSingleton 语义注册默认选择器，
    /// 最终生效的必须是 NoelleConsumerServiceSelector。
    /// 该测试用于锁定对 CAP 内部注册语义的依赖——若 CAP 升级后改变注册方式，此测试会失败报警。
    /// </summary>
    [Fact]
    public void UseCap_WithStorageConfigured_CustomSelectorShouldWinOverCapDefaultSelector()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new DistributedEventBusConfiguration(services);

        // Act：完整模拟真实应用（UseCap + 存储配置，顺序与框架文档一致）
        configuration.UseCap(x => x.UseInMemoryStorage());

        // Assert：IConsumerServiceSelector 仅有一条注册，且为自定义选择器
        var registrations = services
            .Where(s => s.ServiceType == typeof(IConsumerServiceSelector))
            .ToList();

        Assert.Single(registrations);
        Assert.Equal(typeof(CAP.NoelleConsumerServiceSelector), registrations[0].ImplementationType);
    }

    #endregion
}
