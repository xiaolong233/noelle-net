namespace NoelleNet.Ddd.Domain.Entities;

/// <summary>
/// 仓储基类
/// </summary>
public interface INoelleRepository<T> where T : IAggregateRoot
{
    /// <summary>
    /// 保存所有更改
    /// </summary>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}