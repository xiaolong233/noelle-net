using Noelle.Todo.WebApi.Application.Commands;
using Noelle.Todo.WebApi.Application.Models.TodoItems;
using Noelle.Todo.WebApi.Application.Queries.TodoItems;

namespace Noelle.Todo.WebApi.Controllers
{
    /// <summary>
    /// 待办事项
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly ITodoItemQueries _queries;
        private readonly IMediator _mediator;

        /// <summary>
        /// 创建一个新的 <see cref="TodoItemsController"/> 实例
        /// </summary>
        /// <param name="queries"><see cref="ITodoItemQueries"/> 实例</param>
        /// <param name="mediator"><see cref="IMediator"/> 实例</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TodoItemsController(ITodoItemQueries queries, IMediator mediator)
        {
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(queries));
        }

        /// <summary>
        /// 获取待办事项列表
        /// </summary>
        /// <param name="dto">待办事项分页和排序的数据传输对象</param>
        /// <param name="cancellationToken">传播取消操作的通知</param>
        /// <returns></returns>
        [HttpGet]
        public Task<PaginationResultDto<TodoItemDto>> GetListAsync([FromQuery] TodoItemPaginationAndSortDto dto, CancellationToken cancellationToken = default)
        {
            return _queries.GetListAsync(dto, cancellationToken);
        }

        /// <summary>
        /// 获取指定标识符的待办事项
        /// </summary>
        /// <param name="id">待办事项的标识符</param>
        /// <param name="cancellationToken">传播取消操作的通知</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        public Task<TodoItemDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _queries.GetByIdAsync(id, cancellationToken);
        }

        /// <summary>
        /// 创建一个新的待办事项
        /// </summary>
        /// <param name="command">创建待办事项的命令</param>
        /// <param name="cancellationToken">传播取消操作的通知</param>
        /// <returns></returns>
        [HttpPost]
        public Task<EntityDto<Guid>> CreateAsync(CreateTodoItemCommand command, CancellationToken cancellationToken = default)
        {
            return _mediator.Send(command, cancellationToken);
        }

        /// <summary>
        /// 更新待办事项
        /// </summary>
        /// <param name="id">待办事项的标识符</param>
        /// <param name="dto">更新待办事项的数据传输对象</param>
        /// <param name="cancellationToken">传播取消操作的通知</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{id}")]
        public Task UpdateAsync([FromRoute] Guid id, [FromBody] UpdateTodoItemDto dto, CancellationToken cancellationToken = default)
        {
            return _mediator.Send(new UpdateTodoItemCommand(id, dto), cancellationToken);
        }

        /// <summary>
        /// 删除指定标识符的待办事项
        /// </summary>
        /// <param name="id">待办事项的标识符</param>
        /// <param name="cancellationToken">传播取消操作的通知</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{id}")]
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _mediator.Send(new DeleteTodoItemCommand(id), cancellationToken);
        }

        /// <summary>
        /// 完成指定待办事项
        /// </summary>
        /// <param name="id">待办事项的标识符</param>
        /// <param name="cancellationToken">传播取消操作的通知</param>
        /// <returns></returns>
        [HttpPost]
        [Route("{id}/completion")]
        public Task CompleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _mediator.Send(new CompleteTodoItemCommand(id), cancellationToken);
        }
    }
}
