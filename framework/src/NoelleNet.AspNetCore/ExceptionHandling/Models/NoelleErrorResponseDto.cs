namespace NoelleNet.AspNetCore.ExceptionHandling.Models;

/// <summary>
/// 错误结果响应对象
/// </summary>
/// <param name="Error"><see cref="NoelleErrorDetailDto"/> 对象</param>
public record NoelleErrorResponseDto(NoelleErrorDetailDto Error);