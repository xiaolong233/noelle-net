namespace NoelleNet.Auditing;

/// <summary>
/// 定义了包含修改审计信息的接口
/// </summary>
/// <typeparam name="TLastModifiedBy">最后修改者的标识符的类型</typeparam>
public interface IModificationAuditable<TLastModifiedBy>
{
    /// <summary>
    /// 最后修改时间
    /// </summary>
    DateTime? LastModifiedAt { get; }

    /// <summary>
    /// 最后修改人
    /// </summary>
    TLastModifiedBy? LastModifiedBy { get; }
}
