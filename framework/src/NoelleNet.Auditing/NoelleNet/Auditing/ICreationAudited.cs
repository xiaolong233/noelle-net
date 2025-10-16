namespace NoelleNet.Auditing;

/// <summary>
/// 表示具有创建审计信息的对象
/// </summary>
public interface ICreationAudited : IHasCreatedAt, IMayHaveCreator
{
}