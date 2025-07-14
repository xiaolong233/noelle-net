namespace NoelleNet.Http;

/// <summary>
/// 错误响应的数据传输对象
/// </summary>
/// <param name="Error"><see cref="NoelleErrorDto"/> 对象</param>
public record NoelleErrorResponseDto(NoelleErrorDto Error);