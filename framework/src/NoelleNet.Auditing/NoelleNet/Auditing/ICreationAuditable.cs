namespace NoelleNet.Auditing;

/// <summary>
/// 定义了包含创建审计信息的实体接口
/// </summary>
/// <typeparam name="TCreatedBy">创建者的标识符的类型</typeparam>
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
}
