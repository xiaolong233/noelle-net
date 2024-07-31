using MediatR;
using Noelle.Todo.WebApi.Application.Models;

namespace Noelle.Todo.WebApi.Application.Commands;

/// <summary>
/// 创建AccessToken命令
/// </summary>
/// <param name="ChannelId">渠道Id，由平台方提供</param>
/// <param name="Secret">密钥，由平台方提供</param>
public record CreateAccessTokenCommand(string ChannelId, string Secret) : IRequest<CreateAccessTokenResultDto>;