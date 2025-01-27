using NoelleNet.Ddd.Domain.Entities;

namespace NoelleNet.Ddd.Domain.Repositories;

/// <summary>
/// 仓储基类
/// </summary>
/// <typeparam name="T">实体类型，必须实现 <see cref="IAggregateRoot"/> 接口</typeparam>
public interface INoelleRepository<T> where T : IAggregateRoot
{
    /// <summary>
    /// 工作单元
    /// </summary>
    IUnitOfWork UnitOfWork { get; }
}