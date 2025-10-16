namespace NoelleNet.Auditing;

/// <summary>
/// 表示具有创建人标识符的对象
/// </summary>
public interface IMayHaveCreator
{
    /// <summary>
    /// 创建人的标识符
    /// </summary>
    string? CreatedBy { get; }
}
