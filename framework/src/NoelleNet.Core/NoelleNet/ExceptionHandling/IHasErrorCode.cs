namespace NoelleNet.ExceptionHandling;

/// <summary>
/// 表示具有 ErrorCode 的接口
/// </summary>
public interface IHasErrorCode
{
    /// <summary>
    /// 错误代码
    /// </summary>
    string? ErrorCode { get; }
}
