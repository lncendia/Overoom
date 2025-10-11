using Films.Infrastructure.Web.Films.InputModels;
using FluentValidation;

namespace Films.Infrastructure.Web.Films.Validators;

/// <summary>
/// Fluent-валидатор для <see cref="RateFilmInputModel"/>
/// </summary>
public class RateFilmValidator : AbstractValidator<RateFilmInputModel>
{
    /// <summary>
    /// Инициализирует правила валидации для оценки фильма
    /// </summary>
    public RateFilmValidator()
    {
        RuleFor(x => x.Score)
            .NotEmpty()
            .WithMessage("Поле не должно быть пустым")
            .InclusiveBetween(0, 10)
            .WithMessage("Рейтинг должен быть в диапазоне от 0 до 10");
    }
}