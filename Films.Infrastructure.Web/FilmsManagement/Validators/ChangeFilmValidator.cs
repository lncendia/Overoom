using Films.Infrastructure.Web.FilmsManagement.InputModels;
using FluentValidation;

namespace Films.Infrastructure.Web.FilmsManagement.Validators;

/// <summary>
/// Fluent-валидатор для <see cref="ChangeFilmInputModel"/>
/// </summary>
public class ChangeFilmValidator : AbstractValidator<ChangeFilmInputModel>
{
    /// <summary>
    /// Инициализирует валидатор для модели добавления фильма
    /// </summary>
    public ChangeFilmValidator()
    {
        // Основные поля
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Поле не должно быть пустым")
            .MaximumLength(1500).WithMessage("Не больше 1500 символов");

        RuleFor(x => x.ShortDescription)
            .MaximumLength(500).WithMessage("Не больше 500 символов");

        // Рейтинги
        RuleFor(x => x.RatingKp)
            .InclusiveBetween(0, 10).WithMessage("Рейтинг должен быть в диапазоне от 0 до 10");

        RuleFor(x => x.RatingImdb)
            .InclusiveBetween(0, 10).WithMessage("Рейтинг должен быть в диапазоне от 0 до 10");
    }
}