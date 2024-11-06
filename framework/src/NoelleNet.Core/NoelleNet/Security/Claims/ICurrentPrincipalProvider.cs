using System.Security.Claims;

namespace NoelleNet.Security.Claims;

/// <summary>
/// 提供当前用户的 <see cref="ClaimsPrincipal"/> 实例的接口。
/// 该接口用于获取当前的主权信息，包括用户的身份、角色和声明等。
/// </summary>
public interface ICurrentPrincipalProvider
{
    /// <summary>
    /// 获取当前用户的 <see cref="ClaimsPrincipal"/> 实例。
    /// <see cref="ClaimsPrincipal"/> 代表当前线程中的用户及其相关声明和角色。
    /// </summary>
    ClaimsPrincipal? Principal { get; }
}