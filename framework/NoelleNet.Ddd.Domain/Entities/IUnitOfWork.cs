namespace NoelleNet.Ddd.Domain.Entities;

/// <summary>
/// 工作单元
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// 保存所有的实体更改，并发布已注册的领域事件
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}