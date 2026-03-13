namespace Noelle.Todo.WebApi.Application.Permissions;

/// <summary>
/// 系统权限表
/// </summary>
public static class SystemPermissions
{
    /// <summary>
    /// 权限前缀
    /// </summary>
    public const string Prefix = "sys";

    /// <summary>
    /// 数据访问权限
    /// </summary>
    public static class DataAccess
    {
        /// <summary>
        /// 默认权限
        /// </summary>
        public const string Default = Prefix + ".data_access";

        /// <summary>
        /// 本部门
        /// </summary>
        public const string Department = Default + ".department";

        /// <summary>
        /// 本部门及子部门
        /// </summary>
        public const string DepartmentAndSub = Default + ".department_and_sub";

        /// <summary>
        /// 所有
        /// </summary>
        public const string All = Default + ".all";
    }
}
