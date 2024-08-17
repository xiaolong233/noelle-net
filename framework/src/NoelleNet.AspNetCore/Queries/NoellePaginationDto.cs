namespace NoelleNet.AspNetCore.Queries;

/// <summary>
/// 分页查询的参数
/// </summary>
public class NoellePaginationDto
{
    /// <summary>
    /// 跳过多少条记录
    /// </summary>
    public int Offset { get; set; }

    /// <summary>
    /// 返回多少条记录
    /// </summary>
    public int Limit { get; set; } = 10;
}