namespace NoelleNet.Core.Exceptions;

/// <summary>
/// 实现该接口的类会有一个ErrorCode属性
/// </summary>
public interface IHasErrorCode
{
    /// <summary>
    /// 错误代码
    /// </summary>
    string ErrorCode { get; }
}
