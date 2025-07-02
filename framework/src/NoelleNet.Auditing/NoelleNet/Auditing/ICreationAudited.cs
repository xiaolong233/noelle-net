namespace NoelleNet.Auditing;

/// <summary>
/// 表示具有创建审计信息的对象
/// </summary>
public interface ICreationAudited : IHasCreatedAt, IMayHaveCreator
{
}

/// <summary>
/// 表示具有创建审计信息的对象
/// </summary>
/// <typeparam name="TCreatedBy">创建人标识符的数据类型</typeparam>
public interface ICreationAudited<TCreatedBy> : IHasCreatedAt, IMayHaveCreator<TCreatedBy>
{
}
