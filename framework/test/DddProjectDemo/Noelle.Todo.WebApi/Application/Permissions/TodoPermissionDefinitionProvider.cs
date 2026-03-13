namespace Noelle.Todo.WebApi.Application.Permissions;

/// <summary>
/// 服务权限定义提供者
/// </summary>
public static class TodoPermissionDefinitionProvider
{
    private static readonly List<string> _permissions = [];

    /// <summary>
    /// 静态构造函数
    /// </summary>
    static TodoPermissionDefinitionProvider()
    {
        RegisterPermissions(typeof(TodoPermissions));
    }

    /// <summary>
    /// 权限列表
    /// </summary>
    public static IEnumerable<string> Permissions => _permissions.AsReadOnly();

    /// <summary>
    /// 注册权限
    /// </summary>
    /// <param name="type"></param>
    private static void RegisterPermissions(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var innerTypes = type.GetNestedTypes();
        foreach (var innerType in innerTypes)
        {
            var fields = innerType.GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(string) && field.IsLiteral)
                {
                    var value = field.GetValue(null);
                    if (value == null || value is not string str)
                        continue;
                    _permissions.Add((str));
                }
            }
        }
    }
}
