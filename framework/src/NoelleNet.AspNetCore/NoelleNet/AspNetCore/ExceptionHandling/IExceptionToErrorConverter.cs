using NoelleNet.Http;

namespace NoelleNet.AspNetCore.ExceptionHandling;

/// <summary>
/// 异常信息转换器接口，用于将 <see cref="Exception"/> 转换为 <see cref="NoelleErrorDto"/>
/// </summary>
public interface IExceptionToErrorConverter
{
    /// <summary>
    /// <see cref="Exception"/> 转换为 <see cref="NoelleErrorDto"/> 实例
    /// </summary>
    /// <param name="exception">需要进行转换处理的 <see cref="Exception"/> 实例</param>
    /// <param name="optionsBuilder">转换选项构建器</param>
    /// <returns></returns>
    NoelleErrorDto Covert(Exception exception, Action<NoelleExceptionToErrorConvertOptions>? optionsBuilder = null);
}
