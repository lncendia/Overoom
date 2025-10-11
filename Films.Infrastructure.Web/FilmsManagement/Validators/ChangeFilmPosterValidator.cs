using Films.Infrastructure.Web.FilmsManagement.InputModels;
using FluentValidation;

namespace Films.Infrastructure.Web.FilmsManagement.Validators;

/// <summary>
/// Fluent-валидатор для <see cref="ChangeFilmPosterInputModel"/>
/// </summary>
public class ChangeFilmPosterValidator : AbstractValidator<ChangeFilmPosterInputModel>
{
    /// <summary>
    /// Инициализирует валидатор для модели изменения постера фильма
    /// </summary>
    public ChangeFilmPosterValidator()
    {
        RuleFor(x => x.Poster)
            .NotNull().WithMessage("Поле не должно быть пустым")
            .Must(file => file?.ContentType is "image/jpeg" or "image/png")
            .WithMessage("Постер должен быть в формате JPG или PNG");
    }
}