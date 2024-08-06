namespace NoelleNet.Security.Claims;

public class NoelleClaimTypes
{
    /// <summary>
    /// 用于表示客户端标识符的声明类型，默认值：client_id。
    /// </summary>
    public static string ClientId { get; set; } = "client_id";

    /// <summary>
    /// 用于表示用户唯一标识符的声明类型，默认值：sub。
    /// </summary>
    public static string Subject { get; set; } = "sub";

    /// <summary>
    /// 用于表示用户的用户名的声明类型，默认值：username。
    /// </summary>
    public static string UserName { get; set; } = "username";

    /// <summary>
    /// 用于表示用的完整姓名的声明类型，默认值：name。
    /// </summary>
    public static string Name { get; set; } = "name";

    /// <summary>
    /// 用于表示用户的名字（First Name）的声明类型，通常表示用户的名，默认值：given_name。
    /// </summary>
    public static string GivenName { get; set; } = "given_name";

    /// <summary>
    /// 用于表示用户的姓氏（Last Name）的声明类型，通常表示用户的姓，默认值：family_name。
    /// </summary>
    public static string FamilyName { get; set; } = "family_name";

    /// <summary>
    /// 用于表示用户的中间名（Middle Name）的声明类型，通常用于存储用户的中间名或第二个名字，默认值：middle_name。
    /// </summary>
    public static string MiddleName { get; set; } = "middle_name";

    /// <summary>
    /// 用于表示用户昵称的声明类型，默认值：nickname。
    /// </summary>
    public static string NickName { get; set; } = "nickname";

    /// <summary>
    /// 用于表示用户电子邮件地址的声明类型，默认值：email。
    /// </summary>
    public static string Email { get; set; } = "email";

    /// <summary>
    /// 用于表示用户的电子邮件地址是否已验证的声明类型，默认值：email_verified。
    /// </summary>
    public static string EmailVerified { get; set; } = "email_verified";

    /// <summary>
    /// 用于表示用户手机号码的声明类型，默认值：phone_number。
    /// </summary>
    public static string PhoneNumber { get; set; } = "phone_number";

    /// <summary>
    /// 用于表示用户的手机号码是否已验证的声明类型，默认值：phone_number_verified。
    /// </summary>
    public static string PhoneNumberVerified { get; set; } = "phone_number_verified";

    /// <summary>
    /// 用于表示用户性别的声明类型，默认值：。
    /// </summary>
    public static string Gender { get; set; } = "gender";

    /// <summary>
    /// 用于表示用户出生日期的声明类型，默认值：birthdate。
    /// </summary>
    public static string BirthDate { get; set; } = "birthdate";

    /// <summary>
    /// 用于表示用户角色的声明类型，默认值：roles。
    /// </summary>
    public static string Roles { get; set; } = "roles";
}
