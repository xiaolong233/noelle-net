namespace Noelle.Todo.WebApi.Application.Permissions;

/// <summary>
/// 服务权限表
/// </summary>
public static class TodoPermissions
{
    /// <summary>
    /// 权限前缀
    /// </summary>
    public const string Prefix = "todo";

    /// <summary>
    /// 待办事项
    /// </summary>
    public static class TodoItems
    {
        /// <summary>
        /// 默认
        /// </summary>
        private const string Default = Prefix + ".todo_items";

        /// <summary>
        /// 查看
        /// </summary>
        public const string Read = Default + ".read";

        /// <summary>
        /// 编辑
        /// </summary>
        public const string Write = Default + ".write";

        /// <summary>
        /// 删除
        /// </summary>
        public const string Delete = Default + ".delete";
    }
}
