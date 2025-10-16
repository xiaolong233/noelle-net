namespace NoelleNet.Auditing;

/// <summary>
/// 表示具有创建和修改审计信息的对象
/// </summary>
public interface IAudited : ICreationAudited, IModificationAudited
{
}