using Films.Infrastructure.Web.Profile.InputModels;
using FluentValidation;

namespace Films.Infrastructure.Web.Profile.Validators;

/// <summary>
/// Fluent-валидатор для <see cref="GetRatingsInputModel"/>
/// </summary>
public class GetRatingsValidator : AbstractValidator<GetRatingsInputModel>
{
    /// <summary>
    /// Инициализирует правила валидации пагинации рейтингов
    /// </summary>
    public GetRatingsValidator()
    {
        RuleFor(x => x.Take)
            .InclusiveBetween(1, 50)
            .WithMessage("Количество элементов должно быть от 1 до 50");

        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Количество пропускаемых элементов не может быть отрицательным");
    }
}