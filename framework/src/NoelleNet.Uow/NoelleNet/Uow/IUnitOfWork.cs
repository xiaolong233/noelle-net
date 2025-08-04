namespace NoelleNet.Uow;

/// <summary>
/// 工作单元接口
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// 将所有更改保存到数据库
    /// </summary>
    /// <param name="cancellationToken">传播取消操作的通知</param>
    /// <returns></returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
