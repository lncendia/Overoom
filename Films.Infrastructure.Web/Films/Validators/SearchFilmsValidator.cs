using Films.Infrastructure.Web.Films.InputModels;
using FluentValidation;

namespace Films.Infrastructure.Web.Films.Validators;

/// <summary>
/// Fluent-валидатор для <see cref="SearchFilmsInputModel"/>
/// </summary>
public class SearchFilmsValidator : AbstractValidator<SearchFilmsInputModel>
{
    /// <summary>
    /// Инициализирует правила валидации для поиска фильмов
    /// </summary>
    public SearchFilmsValidator()
    {
        // Валидация пагинации
        RuleFor(x => x.Take)
            .InclusiveBetween(1, 50)
            .WithMessage("Количество элементов должно быть от 1 до 50");

        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Количество пропускаемых элементов не может быть отрицательным");

        // Валидация строковых параметров
        RuleFor(x => x.Query)
            .MaximumLength(100)
            .WithMessage("Поисковый запрос не должен превышать 100 символов");

        RuleFor(x => x.Genre)
            .MaximumLength(50)
            .WithMessage("Название жанра не должно превышать 50 символов");

        RuleFor(x => x.Person)
            .MaximumLength(100)
            .WithMessage("Имя участника не должно превышать 100 символов");

        RuleFor(x => x.Country)
            .MaximumLength(50)
            .WithMessage("Название страны не должно превышать 50 символов");

        // Валидация диапазона годов
        When(x => x.MinYear.HasValue && x.MaxYear.HasValue, () => 
        {
            RuleFor(x => x.MinYear)
                .LessThanOrEqualTo(x => x.MaxYear)
                .WithMessage("Минимальный год должен быть меньше или равен максимальному");
        });
        
        // Валидация PlaylistId при наличии значения
        RuleFor(x => x.PlaylistId)
            .NotEmpty()
            .When(x => x.PlaylistId.HasValue)
            .WithMessage("Идентификатор подборки должен быть валидным GUID");
    }
}