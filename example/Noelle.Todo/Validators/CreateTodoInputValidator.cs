using FluentValidation;
using Noelle.Todo.Dtos;

namespace Noelle.Todo.Validators;

/// <summary>
/// 创建待办事项的业务规则验证器（由 NoelleFluentValidationFilter 自动调用）
/// </summary>
public class CreateTodoInputValidator : AbstractValidator<CreateTodoInput>
{
    public CreateTodoInputValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("标题不能为空")
            .MaximumLength(100).WithMessage("标题长度不能超过 100 个字符");
    }
}
