using MediatR;
using Microsoft.AspNetCore.Mvc;
using Noelle.Todo.WebApi.Application.Commands;
using Noelle.Todo.WebApi.Application.Models;

namespace Noelle.Todo.WebApi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/auth/[controller]")]
    [ApiController]
    public class AccessTokensController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// 创建AccessToken
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        public Task<CreateAccessTokenResultDto> CreateAccessToken(CreateAccessTokenCommand command)
        {
            return _mediator.Send(command);
        }
    }
}
