using System.Reflection;

namespace NoelleNet.EventBus;

/// <summary>
/// 程序集类型加载的共享助手：
/// 安全地获取程序集中可加载的类型，跳过无法加载的类型，避免 <see cref="ReflectionTypeLoadException"/> 导致应用启动失败
/// </summary>
internal static class AssemblyTypeLoader
{
    /// <summary>
    /// 安全地获取程序集中可加载的类型，跳过无法加载的类型
    /// </summary>
    /// <param name="assembly">目标程序集</param>
    /// <returns>可加载的类型数组</returns>
    public static Type[] GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null).ToArray()!;
        }
    }
}
