namespace NoelleNet.Application.Dtos;

/// <summary>
/// 定义一个表示实体数据传输对象的接口。
/// </summary>
public interface IEntityDto
{
}

/// <summary>
/// 定义一个表示带有标识符的实体数据传输对象的接口。
/// </summary>
/// <typeparam name="TKey">实体标识符的类型</typeparam>
public interface IEntityDto<TKey> : IEntityDto
{
    /// <summary>
    /// 实体的标识符
    /// </summary>
    TKey Id { get; set; }
}
