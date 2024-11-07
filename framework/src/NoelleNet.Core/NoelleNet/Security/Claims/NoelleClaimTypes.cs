using System.Security.Claims;

namespace NoelleNet.Security.Claims;

/// <summary>
/// 定义声明类型
/// </summary>
public static class NoelleClaimTypes
{
    /// <summary>
    /// 获取或设置客户端标识符的声明类型，默认值：client_id。
    /// </summary>
    public static string ClientId { get; set; } = "client_id";

    /// <summary>
    /// 获取或设置用户唯一标识符的声明类型，默认值：<see cref="ClaimTypes.NameIdentifier"/>。
    /// </summary>
    public static string UserId { get; set; } = ClaimTypes.NameIdentifier;

    /// <summary>
    /// 获取或设置用户名的声明类型，默认值：<see cref="ClaimTypes.Name"/>。
    /// </summary>
    public static string UserName { get; set; } = ClaimTypes.Name;

    /// <summary>
    /// 获取或设置名字的声明类型，通常表示用户的名，默认值：<see cref="ClaimTypes.GivenName"/>。
    /// </summary>
    public static string GivenName { get; set; } = ClaimTypes.GivenName;

    /// <summary>
    /// 获取或设置姓氏的声明类型，通常表示用户的姓，默认值：<see cref="ClaimTypes.Surname"/>。
    /// </summary>
    public static string Surname { get; set; } = ClaimTypes.Surname;

    /// <summary>
    /// 获取或设置用户昵称的声明类型，默认值：nickname。
    /// </summary>
    public static string NickName { get; set; } = "nickname";

    /// <summary>
    /// 获取或设置用户电子邮箱地址的声明类型，默认值：<see cref="ClaimTypes.Email"/>。
    /// </summary>
    public static string Email { get; set; } = ClaimTypes.Email;

    /// <summary>
    /// 获取或设置用户的电子邮箱地址是否已验证的声明类型，默认值：email_verified。
    /// </summary>
    public static string EmailVerified { get; set; } = "email_verified";

    /// <summary>
    /// 获取或设置用户手机号码的声明类型，默认值：phone_number。
    /// </summary>
    public static string PhoneNumber { get; set; } = "phone_number";

    /// <summary>
    /// 获取或设置用户的手机号码是否已验证的声明类型，默认值：phone_number_verified。
    /// </summary>
    public static string PhoneNumberVerified { get; set; } = "phone_number_verified";

    /// <summary>
    /// 获取或设置用户性别的声明类型，默认值：<see cref="ClaimTypes.Gender"/>。
    /// </summary>
    public static string Gender { get; set; } = ClaimTypes.Gender;

    /// <summary>
    /// 获取或设置用户出生日期的声明类型，默认值：<see cref="ClaimTypes.DateOfBirth"/>。
    /// </summary>
    public static string DateOfBirth { get; set; } = ClaimTypes.DateOfBirth;

    /// <summary>
    /// 获取或设置用户角色的声明类型，默认值：<see cref="ClaimTypes.Role"/>。
    /// </summary>
    public static string Role { get; set; } = ClaimTypes.Role;

    /// <summary>
    /// 获取或设置用户权限的声明类型，默认值：permission。
    /// </summary>
    public static string Permission { get; set; } = "permission";

    /// <summary>
    /// 获取或设置用户的数据访问范围的声明类型，默认值：data_access_scope。
    /// </summary>
    public static string DataAccessScope { get; set; } = "data_access_scope";

    /// <summary>
    /// 获取或设置用户可以访问的部门的声明类型，默认值：accessible_dept。
    /// </summary>
    public static string AccessibleDept { get; set; } = "accessible_dept";

    /// <summary>
    /// 获取或设置用户所属部门唯一标识的声明类型，默认值：dept_id。
    /// </summary>
    public static string DeptId { get; set; } = "dept_id";
}
