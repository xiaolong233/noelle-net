namespace NoelleNet.ExceptionHandling;

/// <summary>
/// 定义包含错误详情的接口
/// </summary>
public interface IHasErrorDetails
{
    /// <summary>
    /// 错误详情
    /// </summary>
    string? Details { get; set; }
}
