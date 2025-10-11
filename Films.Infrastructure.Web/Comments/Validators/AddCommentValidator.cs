using Films.Infrastructure.Web.Comments.InputModels;
using FluentValidation;

namespace Films.Infrastructure.Web.Comments.Validators;

/// <summary>
/// Fluent-валидатор для <see cref="AddCommentInputModel"/>
/// </summary>
public class AddCommentValidator : AbstractValidator<AddCommentInputModel>
{
    /// <summary>
    /// Инициализирует новый экземпляр валидатора для добавления комментария
    /// </summary>
    public AddCommentValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Текст комментария не может быть пустым")
            .MaximumLength(1000).WithMessage("Комментарий не должен превышать 1000 символов");
    }
}