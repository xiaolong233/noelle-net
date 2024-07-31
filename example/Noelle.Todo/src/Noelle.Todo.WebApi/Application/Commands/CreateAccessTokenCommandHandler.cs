using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Noelle.Todo.WebApi.Application.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Noelle.Todo.WebApi.Application.Commands;

/// <summary>
/// 创建AccessToken命令的处理器
/// </summary>
public class CreateAccessTokenCommandHandler(IOptionsSnapshot<JwtOptions> jwtOptions) : IRequestHandler<CreateAccessTokenCommand, CreateAccessTokenResultDto>
{
    private readonly IOptionsSnapshot<JwtOptions> _jwtOptions = jwtOptions;

    /// <summary>
    /// 处理命令
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task<CreateAccessTokenResultDto> Handle(CreateAccessTokenCommand request, CancellationToken cancellationToken)
    {
        List<Claim> claims = [new Claim("ChannelId", request.ChannelId)];
        byte[] keyBuffer = Encoding.UTF8.GetBytes(_jwtOptions.Value.SecurityKey);
        DateTime exp = DateTime.Now.AddSeconds(_jwtOptions.Value.ExpireSeconds);
        var secKey = new SymmetricSecurityKey(keyBuffer);
        var credentials = new SigningCredentials(secKey, SecurityAlgorithms.HmacSha256Signature);
        var tokenDescriptor = new JwtSecurityToken(claims: claims, expires: exp, signingCredentials: credentials);
        string jwt = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        CreateAccessTokenResultDto result = new CreateAccessTokenResultDto(jwt, "Bearer", _jwtOptions.Value.ExpireSeconds);
        return Task.FromResult(result);
    }
}
