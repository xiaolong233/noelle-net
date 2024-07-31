using FluentValidation;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace NoelleNet.AspNetCore.MediatR.Behaviors;

/// <summary>
/// 模型验证管道
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class ValidatorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="validators"></param>
    public ValidatorBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    /// <summary>
    /// 处理函数
    /// </summary>
    /// <param name="request"></param>
    /// <param name="next"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NebulaNet.Exceptions.ValidationException"></exception>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        IEnumerable<ValidationResult> results = _validators.Select(v => v.Validate(request))
                                                           .SelectMany(s => s.Errors)
                                                           .Select(s => new ValidationResult(s.ErrorMessage, new List<string>() { s.PropertyName }));

        if (results.Any())
            throw new NoelleNet.AspNetCore.Validation.ValidationException(results);

        return await next();
    }
}
