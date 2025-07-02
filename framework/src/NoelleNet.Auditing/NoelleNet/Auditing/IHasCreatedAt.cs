namespace NoelleNet.Auditing;

/// <summary>
/// 表示具有创建时间的对象
/// </summary>
public interface IHasCreatedAt
{
    /// <summary>
    /// 创建时间
    /// </summary>
    DateTime CreatedAt { get; }
}
