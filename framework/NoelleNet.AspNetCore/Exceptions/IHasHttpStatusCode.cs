namespace NoelleNet.AspNetCore.Exceptions;

/// <summary>
/// 实现该接口的类会有一个HttpStatusCode属性
/// </summary>
public interface IHasHttpStatusCode
{
    /// <summary>
    /// HTTP状态码
    /// </summary>
    int HttpStatusCode { get; }
}