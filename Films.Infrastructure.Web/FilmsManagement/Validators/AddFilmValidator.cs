using Films.Infrastructure.Web.FilmsManagement.InputModels;
using FluentValidation;

namespace Films.Infrastructure.Web.FilmsManagement.Validators;

/// <summary>
/// Fluent-валидатор для <see cref="AddFilmInputModel"/>
/// </summary>
public class AddFilmValidator : AbstractValidator<AddFilmInputModel>
{
    /// <summary>
    /// Инициализирует валидатор для модели добавления фильма
    /// </summary>
    /// <param name="actorValidator">Валидатор для актёров</param>
    public AddFilmValidator(IValidator<ActorInputModel> actorValidator)
    {
        // Основные поля
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Поле не должно быть пустым")
            .MaximumLength(1500).WithMessage("Не больше 1500 символов");

        RuleFor(x => x.ShortDescription)
            .MaximumLength(500).WithMessage("Не больше 500 символов");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Поле не должно быть пустым")
            .MaximumLength(200).WithMessage("Не больше 200 символов");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Поле не должно быть пустым")
            .InclusiveBetween(new DateOnly(1800, 1, 1), new DateOnly(2100, 1, 1))
            .WithMessage("Введите корректный год выхода");

        // Рейтинги
        RuleFor(x => x.RatingKp)
            .InclusiveBetween(0, 10).WithMessage("Рейтинг должен быть в диапазоне от 0 до 10");

        RuleFor(x => x.RatingImdb)
            .InclusiveBetween(0, 10).WithMessage("Рейтинг должен быть в диапазоне от 0 до 10");

        RuleFor(x => x.Countries)
            .NotEmpty().WithMessage("Должна быть хотя бы одна страна")
            .ForEach(x => x.MaximumLength(50).WithMessage("Название страны не должно превышать 50 символов"));

        RuleFor(x => x.Actors)
            .NotEmpty().WithMessage("Должен быть хотя бы один актёр")
            .ForEach(x => x.SetValidator(actorValidator));

        RuleFor(x => x.Directors)
            .NotEmpty().WithMessage("Должен быть хотя бы один режиссёр")
            .ForEach(x => x.MaximumLength(100).WithMessage("Имя режиссёра не должно превышать 100 символов"));

        RuleFor(x => x.Genres)
            .NotEmpty().WithMessage("Должен быть хотя бы один жанр")
            .ForEach(x => x.MaximumLength(50).WithMessage("Название жанра не должно превышать 50 символов"));

        RuleFor(x => x.Screenwriters)
            .NotEmpty().WithMessage("Должен быть хотя бы один сценарист")
            .ForEach(x => x.MaximumLength(100).WithMessage("Имя сценариста не должно превышать 100 символов"));
    }
}