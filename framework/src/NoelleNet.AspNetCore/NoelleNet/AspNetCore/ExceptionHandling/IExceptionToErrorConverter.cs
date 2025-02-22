﻿using NoelleNet.Http;

namespace NoelleNet.AspNetCore.ExceptionHandling;

/// <summary>
/// 异常信息转换器接口，用于将 <see cref="Exception"/> 转换为 <see cref="NoelleErrorDetailDto"/>
/// </summary>
public interface IExceptionToErrorConverter
{
    /// <summary>
    /// <see cref="Exception"/> 转换为 <see cref="NoelleErrorDetailDto"/> 实例
    /// </summary>
    /// <param name="exception">需要进行转换处理的 <see cref="Exception"/> 实例</param>
    /// <returns></returns>
    NoelleErrorDetailDto Covert(Exception exception);
}
