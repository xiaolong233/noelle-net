namespace NoelleNet.Auditing;

/// <summary>
/// 表示具有创建和修改审计信息的对象
/// </summary>
public interface IAudited : ICreationAudited, IModificationAudited
{
}

/// <summary>
/// 表示具有创建和修改审计信息的对象
/// </summary>
/// <typeparam name="TOperatorId">操作人标识符的数据类型</typeparam>
public interface IAudited<TOperatorId> : ICreationAudited<TOperatorId>, IModificationAudited<TOperatorId>
{
}
