namespace NoelleNet.Auditing;

/// <summary>
/// 定义了包含修改审计信息的实体接口
/// </summary>
/// <typeparam name="TLastModifiedBy">表示修改实体的用户的类型</typeparam>
public interface IModificationAuditable<TLastModifiedBy>
{
    /// <summary>
    /// 最后修改时间
    /// </summary>
    DateTime? LastModifiedAt { get; }

    /// <summary>
    /// 最后修改人
    /// </summary>
    TLastModifiedBy LastModifiedBy { get; }

    /// <summary>
    /// 更新修改审计
    /// </summary>
    /// <param name="lastModifiedAt">最后修改时间</param>
    /// <param name="lastModifiedBy">最后修改人</param>
    void UpdateModificationAudit(DateTime lastModifiedAt, TLastModifiedBy lastModifiedBy);
}
