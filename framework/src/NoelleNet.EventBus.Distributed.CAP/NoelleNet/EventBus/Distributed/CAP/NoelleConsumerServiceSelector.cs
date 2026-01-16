using DotNetCore.CAP;
using DotNetCore.CAP.Internal;
using Microsoft.Extensions.DependencyInjection;
using NoelleNet.EventBus.Abstractions;
using NoelleNet.EventBus.Abstractions.Distributed;
using System.Reflection;

namespace NoelleNet.EventBus.Distributed.CAP;

/// <summary>
/// 重写了 <see cref="ConsumerServiceSelector"/>
/// </summary>
public class NoelleConsumerServiceSelector : ConsumerServiceSelector
{
    /// <summary>
    /// 创建一个新的 <see cref="NoelleConsumerServiceSelector"/> 实例
    /// </summary>
    /// <param name="serviceProvider"><see cref="IServiceProvider"/> 实例</param>
    public NoelleConsumerServiceSelector(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <inheritdoc/>
    protected override IEnumerable<ConsumerExecutorDescriptor> FindConsumersFromInterfaceTypes(IServiceProvider provider)
    {
        var descriptors = base.FindConsumersFromInterfaceTypes(provider).ToList();

        using var scope = provider.CreateScope();
        var scopeProvider = scope.ServiceProvider;
        var serviceCollection = scopeProvider.GetRequiredService<IServiceCollection>();

        // 获取所有EventHandler
        var handleInterfaceType = typeof(IEventHandler).GetTypeInfo();
        var handlerTypes = serviceCollection
            .Where(x => x.ImplementationType != null && handleInterfaceType.IsAssignableFrom(x.ImplementationType))
            .Select(x => x.ImplementationType!) ?? [];

        foreach (var handlerType in handlerTypes)
        {
            var interfaceTypes = handlerType.GetInterfaces().Where(x => x.IsGenericType) ?? [];
            foreach (var interfaceType in interfaceTypes)
            {
                if (!(interfaceType.GetGenericTypeDefinition() == typeof(IDistributedEventHandler<>)))
                {
                    continue;
                }

                var genericArgs = interfaceType.GetGenericArguments();
                var innerDescriptors = GetHandlerDescription(genericArgs[0], handlerType);
                descriptors.AddRange(innerDescriptors);
            }
        }

        return descriptors;
    }

    /// <summary>
    /// 获取指定事件类型和处理程序类型的描述信息
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <param name="handlerType">处理程序类型</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    protected virtual IEnumerable<ConsumerExecutorDescriptor> GetHandlerDescription(Type eventType, Type handlerType)
    {
        var serviceTypeInfo = typeof(IDistributedEventHandler<>).MakeGenericType(eventType);
        var method = handlerType.GetMethod(nameof(IDistributedEventHandler<object>.HandleAsync)) ?? throw new InvalidOperationException($"Could not get handle method for {handlerType}");
        var eventNameAttribute = eventType.GetCustomAttribute<EventNameAttribute>(true);
        if (string.IsNullOrWhiteSpace(eventNameAttribute?.Name))
            throw new InvalidOperationException("Event name cannot be empty");
        var topicAttr = method.GetCustomAttributes<TopicAttribute>(true);
        var topicAttributes = topicAttr.ToList();

        if (topicAttributes.Count == 0)
        {
            var capSubscribeAttribute = new CapSubscribeAttribute(eventNameAttribute.Name);
            if (!string.IsNullOrWhiteSpace(eventNameAttribute.Group))
                capSubscribeAttribute.Group = eventNameAttribute.Group;

            topicAttributes.Add(capSubscribeAttribute);
        }

        foreach (var attr in topicAttributes)
        {
            SetSubscribeAttribute(attr);

            var parameters = method.GetParameters()
                .Select(parameter => new ParameterDescriptor
                {
                    Name = parameter.Name!,
                    ParameterType = parameter.ParameterType,
                    IsFromCap = parameter.GetCustomAttributes<FromCapAttribute>().Any()
                                || typeof(CancellationToken).IsAssignableFrom(parameter.ParameterType)
                }).ToList();

            yield return InitDescriptor(attr, method, handlerType.GetTypeInfo(), serviceTypeInfo.GetTypeInfo(), parameters);
        }
    }

    private static ConsumerExecutorDescriptor InitDescriptor(
        TopicAttribute attr,
        MethodInfo methodInfo,
        TypeInfo implType,
        TypeInfo serviceTypeInfo,
        IList<ParameterDescriptor> parameters)
    {
        var descriptor = new ConsumerExecutorDescriptor
        {
            Attribute = attr,
            MethodInfo = methodInfo,
            ImplTypeInfo = implType,
            ServiceTypeInfo = serviceTypeInfo,
            Parameters = parameters
        };

        return descriptor;
    }
}
