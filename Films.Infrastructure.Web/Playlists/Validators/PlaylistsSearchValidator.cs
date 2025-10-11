using Films.Infrastructure.Web.Playlists.InputModels;
using FluentValidation;

namespace Films.Infrastructure.Web.Playlists.Validators;

/// <summary>
/// Fluent-валидатор для <see cref="SearchPlaylistsInputModel"/>
/// </summary>
public class PlaylistsSearchValidator : AbstractValidator<SearchPlaylistsInputModel>
{
    /// <summary>
    /// Инициализирует правила валидации для поиска плейлистов
    /// </summary>
    public PlaylistsSearchValidator()
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
        
        // Валидация FilmId при наличии значения
        RuleFor(x => x.FilmId)
            .NotEmpty()
            .When(x => x.FilmId.HasValue)
            .WithMessage("Идентификатор фильма должен быть валидным GUID");
    }
}