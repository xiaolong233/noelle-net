namespace NoelleNet.Auditing;

/// <summary>
/// 表示具有创建人标识符的对象
/// </summary>
public interface IMayHaveCreator
{
    /// <summary>
    /// 创建人的标识符
    /// </summary>
    Guid? CreatedBy { get; }
}

/// <summary>
/// 表示具有创建人标识符的对象
/// </summary>
/// <typeparam name="TCreatedBy">创建人标识符的数据类型</typeparam>
public interface IMayHaveCreator<TCreatedBy>
{
    /// <summary>
    /// 创建人的标识符
    /// </summary>
    TCreatedBy? CreatedBy { get; }
}
