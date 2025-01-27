namespace NoelleNet.Auditing;

/// <summary>
/// 定义了包含创建和修改审计信息的接口
/// </summary>
/// <typeparam name="TUser">用户标识符的类型</typeparam>
public interface IAuditableEntity<TUser> : ICreationAuditable<TUser>, IModificationAuditable<TUser>
{
}
