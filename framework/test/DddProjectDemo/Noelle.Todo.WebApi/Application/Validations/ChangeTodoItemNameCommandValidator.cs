using FluentValidation;
using Noelle.Todo.WebApi.Application.Commands;

namespace Noelle.Todo.WebApi.Application.Validations;

/// <summary>
/// 修改待办事项名称命令的模型验证器
/// </summary>
public class ChangeTodoItemNameCommandValidator : AbstractValidator<ChangeTodoItemNameCommand>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public ChangeTodoItemNameCommandValidator()
    {
        RuleFor(s => s.NewName).NotNull().NotEmpty().WithMessage("新的待办事项名称为空");
    }
}
