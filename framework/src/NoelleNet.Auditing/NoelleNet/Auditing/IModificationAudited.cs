namespace NoelleNet.Auditing;

/// <summary>
/// 表示具有修改审计信息的对象
/// </summary>
public interface IModificationAudited
{
    /// <summary>
    /// 最后修改时间
    /// </summary>
    DateTime? LastModifiedAt { get; }

    /// <summary>
    /// 最后修改人的标识符
    /// </summary>
    Guid? LastModifiedBy { get; }
}

/// <summary>
/// 表示具有修改审计信息的对象
/// </summary>
/// <typeparam name="TLastModifiedBy">最后修改人标识符的数据类型</typeparam>
public interface IModificationAudited<TLastModifiedBy>
{
    /// <summary>
    /// 最后修改时间
    /// </summary>
    DateTime? LastModifiedAt { get; }

    /// <summary>
    /// 最后修改人的标识符
    /// </summary>
    TLastModifiedBy? LastModifiedBy { get; }
}
