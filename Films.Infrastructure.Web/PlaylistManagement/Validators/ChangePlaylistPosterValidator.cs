using Films.Infrastructure.Web.PlaylistManagement.InputModels;
using FluentValidation;

namespace Films.Infrastructure.Web.PlaylistManagement.Validators;

/// <summary>
/// Fluent-валидатор для <see cref="ChangePlaylistPosterInputModel"/>
/// </summary>
public class ChangePlaylistPosterValidator : AbstractValidator<ChangePlaylistPosterInputModel>
{
    /// <summary>
    /// Инициализирует валидатор для модели изменения постера подборки
    /// </summary>
    public ChangePlaylistPosterValidator()
    {
        RuleFor(x => x.Poster)
            .NotNull().WithMessage("Поле не должно быть пустым")
            .Must(file => file?.ContentType is "image/jpeg" or "image/png")
            .WithMessage("Постер должен быть в формате JPG или PNG");
    }
}