using Films.Infrastructure.Web.PlaylistManagement.InputModels;
using FluentValidation;

namespace Films.Infrastructure.Web.PlaylistManagement.Validators;

/// <summary>
/// Fluent-валидатор для <see cref="ChangePlaylistInputModel"/>
/// </summary>
public class ChangePlaylistValidator : AbstractValidator<ChangePlaylistInputModel>
{
    /// <summary>
    /// Инициализирует правила валидации для изменения плейлиста
    /// </summary>
    public ChangePlaylistValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Поле не должно быть пустым")
            .MaximumLength(500).WithMessage("Не больше 500 символов");

        When(x => x.Films != null, () =>
        {
            RuleFor(x => x.Films)
                .NotEmpty().WithMessage("Должен быть хотя бы один фильм")
                .ForEach(filmId => filmId.NotEmpty().WithMessage("Идентификатор фильма должен быть валидным GUID"));
        });
    }
}