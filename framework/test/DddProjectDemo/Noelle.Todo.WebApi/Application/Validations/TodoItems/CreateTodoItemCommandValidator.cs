using FluentValidation;
using Noelle.Todo.Domain.Todo;
using Noelle.Todo.WebApi.Application.Commands;

namespace Noelle.Todo.WebApi.Application.Validations.TodoItems;

/// <summary>
/// <see cref="CreateTodoItemCommand"/> 的模型验证器
/// </summary>
public class CreateTodoItemCommandValidator : AbstractValidator<CreateTodoItemCommand>
{
    private readonly IStringLocalizer<TodoResource> _localizer;

    /// <summary>
    /// 创建一个新的 <see cref="CreateTodoItemCommandValidator"/> 实例
    /// </summary>
    /// <param name="localizer"><see cref="IStringLocalizer{T}"/> 实例</param>
    /// <exception cref="ArgumentNullException"></exception>
    public CreateTodoItemCommandValidator(IStringLocalizer<TodoResource> localizer)
    {
        _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));

        RuleFor(s => s.Name)
            .NotEmpty()
            .WithMessage(_localizer["TodoItem.Name.NullErrorMessage"])
            .MaximumLength(TodoItemConstraints.Name.MaxLength)
            .WithMessage(_localizer["TodoItem.Name.MaxLengthErrorMessage", TodoItemConstraints.Name.MaxLength]);
    }
}
