using System.Security.Claims;

namespace NoelleNet.Security;

/// <summary>
/// 表示当前用户的接口
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// 客户端的唯一标识
    /// </summary>
    string? ClientId { get; }

    /// <summary>
    /// 用户的唯一标识
    /// </summary>
    string? Id { get; }

    /// <summary>
    /// 所属部门的唯一标识
    /// </summary>
    string? DeptId { get; }

    /// <summary>
    /// 用户的用户名
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// 用户姓名的名字部分
    /// </summary>
    string? GivenName { get; }

    /// <summary>
    /// 用户姓名的姓氏部分
    /// </summary>
    string? Surname { get; }

    /// <summary>
    /// 用户的昵称
    /// </summary>
    string? NickName { get; }

    /// <summary>
    /// 用户的电子邮件地址
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// 一个值，指示用户的电子邮件是否已验证
    /// </summary>
    bool EmailConfirmed { get; }

    /// <summary>
    /// 用户的手机号码
    /// </summary>
    string? PhoneNumber { get; }

    /// <summary>
    /// 一个值，指示用户的手机号码是否已验证
    /// </summary>
    bool PhoneNumberConfirmed { get; }

    /// <summary>
    /// 用户的性别
    /// </summary>
    string? Gender { get; }

    /// <summary>
    /// 用户的出生日期
    /// </summary>
    DateTime? DateOfBirth { get; }

    /// <summary>
    /// 用户关联的角色列表
    /// </summary>
    string[] Roles { get; }

    /// <summary>
    /// 判断用户是否具有指定的角色
    /// </summary>
    /// <param name="roleName">要检查的角色名称</param>
    /// <returns>如果用户具有指定角色，则返回 true；否则返回 false</returns>
    bool IsInRole(string roleName);

    /// <summary>
    /// 用户的权限列表
    /// </summary>
    string[] Permissions { get; }

    /// <summary>
    /// 返回一个值，指示用户是否拥有指定的权限
    /// </summary>
    /// <param name="permission">权限名</param>
    /// <returns></returns>
    bool HasPermission(string permission);

    /// <summary>
    /// 当前用户的所有声明
    /// </summary>
    Claim[] Claims { get; }

    /// <summary>
    /// 查找指定类型的声明
    /// </summary>
    /// <param name="claimType">要查找的声明类型</param>
    /// <returns>匹配的声明，如果未找到则返回 null</returns>
    Claim? FindClaim(string claimType);

    /// <summary>
    /// 查找指定类型的声明值
    /// </summary>
    /// <param name="claimType">要查找的声明类型</param>
    /// <returns>如果找到匹配的声明，则返回声明的值；如果未找到匹配的声明，则返回 null</returns>
    string? FindClaimValue(string claimType);

    /// <summary>
    /// 查找指定类型的所有声明
    /// </summary>
    /// <param name="claimType">要查找的声明类型</param>
    /// <returns>匹配的声明数组，如果没有匹配项则返回空数组</returns>
    Claim[] FindClaims(string claimType);

    /// <summary>
    /// 查找指定类型的所有声明值
    /// </summary>
    /// <param name="claimType">要查找的声明类型</param>
    /// <returns></returns>
    string[] FindClaimValues(string claimType);
}
