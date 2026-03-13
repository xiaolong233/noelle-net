using FluentValidation;

namespace Noelle.Todo.WebApi.Application.Validations;

/// <summary>
/// 模型验证的扩展方法集
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// 添加模型验证配置
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> 实例</param>
    /// <returns></returns>
    public static IServiceCollection AddModelValidations(this IServiceCollection services)
    {
        return services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
