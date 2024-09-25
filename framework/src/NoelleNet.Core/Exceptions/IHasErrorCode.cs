namespace NoelleNet.Core.Exceptions;

/// <summary>
/// 实现该接口的类会有一个 ErrorCode 属性
/// </summary>
public interface IHasErrorCode
{
    /// <summary>
    /// 错误代码
    /// </summary>
    string? ErrorCode { get; }
}
