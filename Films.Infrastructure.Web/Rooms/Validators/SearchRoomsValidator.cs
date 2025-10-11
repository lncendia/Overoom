using Films.Infrastructure.Web.Rooms.InputModels;
using FluentValidation;

namespace Films.Infrastructure.Web.Rooms.Validators;

/// <summary>
/// Fluent-валидатор для <see cref="SearchRoomsInputModel"/>
/// </summary>
public class SearchRoomsValidator : AbstractValidator<SearchRoomsInputModel>
{
    /// <summary>
    /// Инициализирует правила валидации для поиска комнат
    /// </summary>
    public SearchRoomsValidator()
    {
        // Валидация пагинации
        RuleFor(x => x.Take)
            .InclusiveBetween(1, 50)
            .WithMessage("Количество элементов должно быть от 1 до 50");

        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Количество пропускаемых элементов не может быть отрицательным");

        // Валидация FilmId при наличии значения
        RuleFor(x => x.FilmId)
            .NotEmpty()
            .When(x => x.FilmId.HasValue)
            .WithMessage("Идентификатор фильма должен быть валидным GUID");
    }
}