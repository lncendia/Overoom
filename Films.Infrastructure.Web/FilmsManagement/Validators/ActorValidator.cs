using Films.Infrastructure.Web.FilmsManagement.InputModels;
using FluentValidation;

namespace Films.Infrastructure.Web.FilmsManagement.Validators;

/// <summary>
/// Fluent-валидатор для <see cref="ActorInputModel"/>
/// </summary>
public class ActorValidator : AbstractValidator<ActorInputModel>
{
    /// <summary>
    /// Валидатор для модели Actor
    /// </summary>
    public ActorValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Имя актёра обязательно")
            .MaximumLength(100).WithMessage("Имя актёра не должно превышать 100 символов");
        
        RuleFor(x => x.Role)
            .MaximumLength(100).WithMessage("Роль не должна превышать 100 символов");
    }
}