using MediatR;
using NoelleNet.EventBus.Abstractions.Local;

namespace NoelleNet.EventBus.Local.MediatR;

/// <summary>
/// 基于 MediatR 实现的本地事件总线
/// </summary>
public class MediatRLocalEventBus : ILocalEventBus
{
    private readonly IMediator _mediator;

    /// <summary>
    /// 创建一个新的 <see cref="MediatRLocalEventBus"/> 实例
    /// </summary>
    /// <param name="mediator"><see cref="IMediator"/> 实例</param>
    /// <exception cref="ArgumentNullException"></exception>
    public MediatRLocalEventBus(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <inheritdoc/>
    public Task PublishAsync<TEvent>(TEvent eventData, CancellationToken cancellationToken = default)
    {
        if (eventData == null)
            throw new ArgumentNullException(nameof(eventData));

        var wrapper = new LocalEventAdapter(eventData);
        return _mediator.Publish(wrapper, cancellationToken);
    }
}
