using Films.Infrastructure.Web.Films.InputModels;
using FluentValidation;

namespace Films.Infrastructure.Web.Films.Validators;

/// <summary>
/// Fluent-валидатор для <see cref="GetPopularFilmsInputModel"/>
/// </summary>
public class GetPopularFilmsValidator : AbstractValidator<GetPopularFilmsInputModel>
{
    /// <summary>
    /// Инициализирует правила валидации для запроса популярных фильмов
    /// </summary>
    public GetPopularFilmsValidator()
    {
        RuleFor(x => x.Take)
            .InclusiveBetween(1, 30)
            .WithMessage("Количество фильмов должно быть от 1 до 30");
    }
}