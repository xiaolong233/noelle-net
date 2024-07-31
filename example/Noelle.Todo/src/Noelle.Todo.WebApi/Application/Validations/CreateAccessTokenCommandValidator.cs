using FluentValidation;
using Noelle.Todo.WebApi.Application.Commands;

namespace Noelle.Todo.WebApi.Application.Validations;

/// <summary>
/// <see cref="CreateAccessTokenCommand"/> 的模型验证器
/// </summary>
public class CreateAccessTokenCommandValidator : AbstractValidator<CreateAccessTokenCommand>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public CreateAccessTokenCommandValidator()
    {
        RuleFor(s => s.ChannelId).NotNull().NotEmpty().WithMessage("渠道Id不能为空");
        RuleFor(s => s.Secret).NotNull().NotEmpty().WithMessage("密钥不能为空");
    }
}
