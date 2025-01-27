namespace NoelleNet;

/// <summary>
/// 定义 <see cref="Guid"/> 生成器接口
/// </summary>
public interface IGuidGenerator
{
    /// <summary>
    /// 生成一个新的 <see cref="Guid"/> 
    /// </summary>
    /// <returns></returns>
    Guid Generate();
}
