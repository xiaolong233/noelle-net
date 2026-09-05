using System.Security.Claims;

namespace NoelleNet.Security.Claims;

/// <summary>
/// 定义声明类型
/// 默认值遵循 OpenID Connect 标准声明（短名），以适配 JWT 令牌生态；
/// 对仅使用 Cookie 认证的应用，可在启动阶段将相关属性覆盖为 <see cref="ClaimTypes"/> 中的 URI 类型
/// </summary>
public static class NoelleClaimTypes
{
    /// <summary>
    /// 获取或设置客户端标识符的声明类型，默认值：client_id
    /// </summary>
    public static string ClientId { get; set; } = "client_id";

    /// <summary>
    /// 获取或设置令牌主体唯一标识符的声明类型，默认值：sub
    /// 用户认证时值为用户唯一标识符，客户端认证时值为客户端标识符
    /// </summary>
    public static string Subject { get; set; } = "sub";

    /// <summary>
    /// 获取或设置用户唯一标识符的声明类型，默认值：user_id，仅用户认证的令牌携带
    /// </summary>
    public static string UserId { get; set; } = "user_id";

    /// <summary>
    /// 获取或设置用户名的声明类型，默认值：preferred_username
    /// </summary>
    public static string UserName { get; set; } = "preferred_username";

    /// <summary>
    /// 获取或设置用户完整姓名的声明类型，默认值：name
    /// </summary>
    public static string Name { get; set; } = "name";

    /// <summary>
    /// 获取或设置名字的声明类型，通常表示用户的名，默认值：given_name
    /// </summary>
    public static string GivenName { get; set; } = "given_name";

    /// <summary>
    /// 获取或设置姓氏的声明类型，通常表示用户的姓，默认值：family_name
    /// </summary>
    public static string Surname { get; set; } = "family_name";

    /// <summary>
    /// 获取或设置用户姓名的中间名部分的声明类型，默认值：middle_name
    /// </summary>
    public static string MiddleName { get; set; } = "middle_name";

    /// <summary>
    /// 获取或设置用户昵称的声明类型，默认值：nickname
    /// </summary>
    public static string NickName { get; set; } = "nickname";

    /// <summary>
    /// 获取或设置用户电子邮箱地址的声明类型，默认值：email
    /// </summary>
    public static string Email { get; set; } = "email";

    /// <summary>
    /// 获取或设置用户的电子邮箱地址是否已验证的声明类型，默认值：email_verified
    /// </summary>
    public static string EmailVerified { get; set; } = "email_verified";

    /// <summary>
    /// 获取或设置用户手机号码的声明类型，默认值：phone_number
    /// </summary>
    public static string PhoneNumber { get; set; } = "phone_number";

    /// <summary>
    /// 获取或设置用户的手机号码是否已验证的声明类型，默认值：phone_number_verified
    /// </summary>
    public static string PhoneNumberVerified { get; set; } = "phone_number_verified";

    /// <summary>
    /// 获取或设置用户性别的声明类型，默认值：gender
    /// </summary>
    public static string Gender { get; set; } = "gender";

    /// <summary>
    /// 获取或设置用户出生日期的声明类型，默认值：birthdate
    /// </summary>
    public static string DateOfBirth { get; set; } = "birthdate";

    /// <summary>
    /// 获取或设置用户角色的声明类型，默认值：role
    /// </summary>
    public static string Role { get; set; } = "role";

    /// <summary>
    /// 获取或设置用户权限的声明类型，默认值：permission
    /// </summary>
    public static string Permission { get; set; } = "permission";

    /// <summary>
    /// 获取或设置用户所属组织单元唯一标识的声明类型，默认值：organization_unit_id
    /// </summary>
    public static string OrganizationUnitId { get; set; } = "organization_unit_id";
}
