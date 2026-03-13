using Noelle.Todo.Domain.Todo;
using Noelle.Todo.WebApi.Application.IntegrationEvents;
using NoelleNet.EventBus.Abstractions.Distributed;

namespace Noelle.Todo.WebApi.Application.Commands;

/// <summary>
/// <see cref="CreateTodoItemCommand"/> 的处理程序
/// </summary>
public class CreateTodoItemCommandHandler : IRequestHandler<CreateTodoItemCommand, EntityDto<Guid>>
{
    private readonly ITodoItemRepository _repository;
    private readonly IDistributedEventBus _distributedEventBus;

    /// <summary>
    /// 创建一个新的 <see cref="CreateTodoItemCommandHandler"/> 实例
    /// </summary>
    /// <param name="repository"><see cref="ITodoItemRepository"/> 实例</param>
    /// <param name="distributedEventBus"><see cref="IDistributedEventBus"/> 实例</param>
    /// <exception cref="ArgumentNullException"></exception>
    public CreateTodoItemCommandHandler(ITodoItemRepository repository, IDistributedEventBus distributedEventBus)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _distributedEventBus = distributedEventBus ?? throw new ArgumentNullException(nameof(distributedEventBus));
    }

    /// <summary>
    /// 命令处理函数
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    public async Task<EntityDto<Guid>> Handle(CreateTodoItemCommand request, CancellationToken cancellationToken)
    {
        var item = new TodoItem(request.Name);

        await _repository.AddTodoItemAsync(item, cancellationToken);

        await _distributedEventBus.PublishAsync(new CreateTodoItemIntegrationEvent(item.Name), cancellationToken);

        return new EntityDto<Guid> { Id = item.Id };
    }
}
