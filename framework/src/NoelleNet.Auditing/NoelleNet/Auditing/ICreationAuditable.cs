namespace NoelleNet.Auditing;

/// <summary>
/// 定义了包含创建审计信息的实体接口
/// </summary>
/// <typeparam name="TCreatedBy">表示创建实体的用户的标识符的类型</typeparam>
public interface ICreationAuditable<TCreatedBy>
{
    /// <summary>
    /// 创建时间
    /// </summary>
    DateTime CreatedAt { get; }

    /// <summary>
    /// 创建人
    /// </summary>
    TCreatedBy? CreatedBy { get; }

    /// <summary>
    /// 设置创建审计
    /// </summary>
    /// <param name="createdAt">创建时间</param>
    /// <param name="createdBy">创建人</param>
    void SetCreationAudit(DateTime createdAt, TCreatedBy? createdBy);
}
