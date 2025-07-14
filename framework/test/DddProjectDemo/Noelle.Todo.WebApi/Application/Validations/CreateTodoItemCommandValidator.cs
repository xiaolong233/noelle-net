using FluentValidation;
using Noelle.Todo.WebApi.Application.Commands;

namespace Noelle.Todo.WebApi.Application.Validations;

/// <summary>
/// 创建待办事项命令的模型验证器
/// </summary>
public class CreateTodoItemCommandValidator : AbstractValidator<CreateTodoItemCommand>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public CreateTodoItemCommandValidator()
    {
        RuleFor(s => s.Name)
            .NotEmpty()
            .WithMessage("待办事项名称为空");
    }
}
