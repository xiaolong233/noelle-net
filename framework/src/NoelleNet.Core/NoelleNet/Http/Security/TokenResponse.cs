namespace NoelleNet.Http.Security;

/// <summary>
/// 令牌响应结果
/// </summary>
/// <param name="AccessToken">访问令牌</param>
/// <param name="ExpiresIn">有效期，单位：秒</param>
public record TokenResponse(string AccessToken, int ExpiresIn);