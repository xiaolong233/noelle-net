using System.Security.Claims;

namespace NoelleNet.Security;

/// <summary>
/// 表示当前用户的接口。此接口定义了获取用户信息和角色的方法和属性。
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// 获取客户端标识符（Client ID），表示当前用户关联的客户端或应用程序的唯一标识。
    /// </summary>
    string? ClientId { get; }

    /// <summary>
    /// 获取用户的唯一标识符（ID）。这是用户的唯一标识符，通常用于在系统中唯一标识用户。
    /// </summary>
    string? Id { get; }

    /// <summary>
    /// 获取用户的用户名。通常用于显示用户名或用于系统中的用户标识。
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// 获取用户的全名。通常用于显示用户的完整姓名。
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// 获取用户的名字（Given Name）。表示用户的名。
    /// </summary>
    string? FirstName { get; }

    /// <summary>
    /// 获取用户的姓氏（Family Name）。表示用户的姓。
    /// </summary>
    string? LastName { get; }

    /// <summary>
    /// 获取用户的中间名。表示用户的中间名或第二个名字。
    /// </summary>
    string? MiddleName { get; }

    /// <summary>
    /// 获取用户的昵称（Nickname）。通常用于表示用户的别名或显示名。
    /// </summary>
    string? NickName { get; }

    /// <summary>
    /// 获取用户的电子邮件地址。用于通信和标识用户。
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// 获取一个值，指示用户的电子邮件是否已验证。
    /// </summary>
    bool EmailConfirmed { get; }

    /// <summary>
    /// 获取用户的手机号码。用于通信或身份验证。
    /// </summary>
    string? PhoneNumber { get; }

    /// <summary>
    /// 获取一个值，指示用户的手机号码是否已验证。
    /// </summary>
    bool PhoneNumberConfirmed { get; }

    /// <summary>
    /// 获取用户的性别。表示用户的性别信息。
    /// </summary>
    string? Gender { get; }

    /// <summary>
    /// 获取用户的出生日期。通常用于用户的个人资料信息。
    /// </summary>
    string? DateOfBirth { get; }

    /// <summary>
    /// 获取用户的角色列表。表示用户在系统中的角色或权限。
    /// </summary>
    string[] Roles { get; }

    /// <summary>
    /// 判断用户是否具有指定的角色。
    /// </summary>
    /// <param name="roleName">要检查的角色名称。</param>
    /// <returns>如果用户具有指定角色，则返回 true；否则返回 false。</returns>
    bool IsInRole(string roleName);

    /// <summary>
    /// 查找指定类型的声明。
    /// </summary>
    /// <param name="claimType">要查找的声明类型。</param>
    /// <returns>匹配的声明，如果未找到则返回 null。</returns>
    Claim? FindClaim(string claimType);

    /// <summary>
    /// 查找指定类型的声明值。
    /// </summary>
    /// <param name="claimType">要查找的声明类型。</param>
    /// <returns>如果找到匹配的声明，则返回声明的值；如果未找到匹配的声明，则返回 null。</returns>
    string? FindClaimValue(string claimType);

    /// <summary>
    /// 查找指定类型的所有声明。
    /// </summary>
    /// <param name="claimType">要查找的声明类型。</param>
    /// <returns>匹配的声明数组，如果没有匹配项则返回空数组。</returns>
    Claim[] FindClaims(string claimType);

    /// <summary>
    /// 获取所有声明。
    /// </summary>
    /// <returns>所有声明的数组。</returns>
    Claim[] GetAllClaims();
}
