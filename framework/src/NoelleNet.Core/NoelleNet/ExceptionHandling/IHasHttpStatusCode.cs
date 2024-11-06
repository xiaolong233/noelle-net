namespace NoelleNet.ExceptionHandling;

/// <summary>
/// 表示具有 StatusCode 的接口
/// </summary>
public interface IHasHttpStatusCode
{
    /// <summary>
    /// HTTP状态码
    /// </summary>
    int StatusCode { get; }
}
