using Films.Infrastructure.Web.Components.Interfaces;
using FluentValidation;

namespace Films.Infrastructure.Web.Components.Validators;

/// <summary>
/// Валидатор для классов, реализующих интерфейс IWithInputPagination
/// </summary>
public class WithInputPaginationValidator : AbstractValidator<IWithInputPagination>
{
    /// <summary>
    /// Конструктор валидатора
    /// </summary>
    public WithInputPaginationValidator()
    {
        // Валидация пагинации
        RuleFor(x => x.Take)
            .InclusiveBetween(1, 50)
            .WithMessage("Количество элементов должно быть от 1 до 50");

        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Количество пропускаемых элементов не может быть отрицательным");
    }
}