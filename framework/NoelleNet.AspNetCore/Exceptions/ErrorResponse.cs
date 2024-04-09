namespace NoelleNet.AspNetCore.Exceptions;

/// <summary>
/// 错误结果响应对象
/// </summary>
/// <param name="Error"><see cref="Exceptions.Error"/> 对象</param>
public record ErrorResponse(Error Error);