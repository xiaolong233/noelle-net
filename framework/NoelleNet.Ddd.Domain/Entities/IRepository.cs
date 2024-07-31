namespace NoelleNet.Ddd.Domain.Entities;

/// <summary>
/// 仓储基类
/// </summary>
public interface IRepository<T> where T : IAggregateRoot
{
    /// <summary>
    /// 工作单元
    /// </summary>
    IUnitOfWork UnitOfWork { get; }
}