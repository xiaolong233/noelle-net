using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Noelle.Todo.Domain.Todo.Entities;
using Noelle.Todo.WebApi.Application.Commands;
using Noelle.Todo.WebApi.Application.Models;
using Noelle.Todo.WebApi.Application.Queries;
using NoelleNet.AspNetCore.Queries;
using NoelleNet.Ddd.Domain.Exceptions;

namespace Noelle.Todo.WebApi.Controllers
{
    /// <summary>
    /// 待办事项控制器
    /// </summary>
    /// <param name="todoItemQueries"></param>
    /// <param name="mediator"></param>
    [Route("api/[controller]")]
    [ApiController]
    public class TodoItemsController(ITodoItemQueries todoItemQueries, IMediator mediator) : ControllerBase
    {
        private readonly ITodoItemQueries _todoItemQueries = todoItemQueries;
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// 获取代办事项列表
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public Task<NoellePaginationAndSortResultDto<TodoItemDto>> GetListAsync([FromQuery] NoellePaginationAndSortDto dto)
        {
            return _todoItemQueries.GetTodoItemsAsync(dto);
        }

        /// <summary>
        /// 获取指定标识符的待办事项的详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NoelleEntityNotFoundException"></exception>
        [HttpGet]
        [Route("{id}")]
        public async Task<TodoItemDto> GetDetailAsync(Guid id)
        {
            return (await _todoItemQueries.GetTodoItemAsync(id)) ?? throw new NoelleEntityNotFoundException(typeof(TodoItem), id);
        }

        /// <summary>
        /// 创建一个新的待办事项
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        public Task<TodoItemDto> CreateAsync(CreateTodoItemCommand command)
        {
            return _mediator.Send(command);
        }

        /// <summary>
        /// 修改待办事项的名称
        /// </summary>
        /// <param name="id">待办事项的主键</param>
        /// <param name="dto">请求参数</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{id}/name")]
        public Task<TodoItemDto> ChangeNameAsync(Guid id, ChangeTodoItemNameRequestDto dto)
        {
            ChangeTodoItemNameCommand command = new(id, dto.NewName);
            return _mediator.Send(command);
        }

        /// <summary>
        /// 删除指定的待办事项
        /// </summary>
        /// <param name="id">待办事项的标识符</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{id}")]
        public Task DeleteAsync(Guid id)
        {
            DeleteTodoItemCommand command = new(id);
            return _mediator.Send(command);
        }

        /// <summary>
        /// 完成指定待办事项
        /// </summary>
        /// <param name="id">待办事项的标识符</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{id}/completion")]
        public Task Completion(Guid id)
        {
            CompleteTodoItemCommand command = new(id);
            return _mediator.Send(command);
        }
    }
}
