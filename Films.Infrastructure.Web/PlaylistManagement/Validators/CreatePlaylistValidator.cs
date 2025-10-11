using Films.Infrastructure.Web.PlaylistManagement.InputModels;
using FluentValidation;

namespace Films.Infrastructure.Web.PlaylistManagement.Validators;

/// <summary>
/// Fluent-валидатор для <see cref="CreatePlaylistInputModel"/>
/// </summary>
public class CreatePlaylistValidator : AbstractValidator<CreatePlaylistInputModel>
{
    /// <summary>
    /// Инициализирует правила валидации для создания плейлиста
    /// </summary>
    public CreatePlaylistValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Поле не должно быть пустым")
            .MaximumLength(200).WithMessage("Не больше 200 символов");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Поле не должно быть пустым")
            .MaximumLength(500).WithMessage("Не больше 500 символов");

        RuleFor(x => x.Films)
            .NotEmpty().WithMessage("Должен быть хотя бы один фильм")
            .ForEach(filmId => filmId.NotEmpty().WithMessage("Идентификатор фильма должен быть валидным GUID"));
    }
}