namespace NoelleNet.Core.Exceptions;

/// <summary>
/// 表示包含HTTP状态码的接口
/// </summary>
public interface IHasHttpStatusCode
{
    /// <summary>
    /// HTTP状态码
    /// </summary>
    int StatusCode { get; }
}
