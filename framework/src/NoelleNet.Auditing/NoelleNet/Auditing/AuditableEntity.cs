namespace NoelleNet.Auditing;

/// <summary>
/// 实现了 <see cref="AuditableEntityBase{TIdentifier, TUser}"/>，TUser 为 <see cref="string?"/>
/// </summary>
/// <typeparam name="TIdentifier">实体标识符的类型</typeparam>
public class AuditableEntity<TIdentifier> : AuditableEntityBase<TIdentifier, string?>
{
}
