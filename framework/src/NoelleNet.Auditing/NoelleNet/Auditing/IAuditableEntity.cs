namespace NoelleNet.Auditing;

/// <summary>
/// 定义了包含创建和修改审计信息的实体接口
/// </summary>
/// <typeparam name="TUser">表示创建和修改实体的用户的标识符的类型</typeparam>
public interface IAuditableEntity<TUser> : ICreationAuditable<TUser>, IModificationAuditable<TUser>
{
}
