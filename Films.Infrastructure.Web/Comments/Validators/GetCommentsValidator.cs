using Films.Infrastructure.Web.Comments.InputModels;
using FluentValidation;

namespace Films.Infrastructure.Web.Comments.Validators;

/// <summary>
/// Fluent-валидатор для <see cref="GetCommentsInputModel"/>
/// </summary>
public class GetCommentsValidator : AbstractValidator<GetCommentsInputModel>
{
    /// <summary>
    /// Инициализирует правила валидации для пагинации комментариев
    /// </summary>
    public GetCommentsValidator()
    {
        RuleFor(x => x.Take)
            .InclusiveBetween(1, 50)
            .WithMessage("Количество элементов должно быть от 1 до 50");

        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Пропуск не может быть отрицательным");
    }
}