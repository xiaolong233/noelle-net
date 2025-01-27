namespace NoelleNet.AspNetCore.Queries;

/// <summary>
/// 分页查询参数的数据传输对象
/// </summary>
public class NoellePaginationDto
{
    /// <summary>
    /// 跳过的记录数
    /// </summary>
    public int Offset { get; set; }

    /// <summary>
    /// 获取的记录数
    /// </summary>
    public int Limit { get; set; } = 10;
}