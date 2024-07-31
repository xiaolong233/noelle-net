namespace Noelle.Todo.WebApi.Application.Models;

/// <summary>
/// 创建AccessToken的结果
/// </summary>
/// <param name="AccessToken">访问令牌</param>
/// <param name="TokenType">令牌类型，默认为"Bearer"</param>
/// <param name="ExpiresIn">访问令牌的有效期，单位：秒，默认值：7200</param>
public record CreateAccessTokenResultDto(string AccessToken, string TokenType, int ExpiresIn);
