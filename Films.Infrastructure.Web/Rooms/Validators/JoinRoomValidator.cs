using Films.Infrastructure.Web.Rooms.InputModels;
using FluentValidation;

namespace Films.Infrastructure.Web.Rooms.Validators;

/// <summary>
/// Fluent-валидатор для <see cref="JoinRoomInputModel"/>
/// </summary>
public class JoinRoomValidator : AbstractValidator<JoinRoomInputModel>
{
    /// <summary>
    /// Инициализирует правила валидации для подключения к комнате
    /// </summary>
    public JoinRoomValidator()
    {
        RuleFor(x => x.Code)
            .Length(5)
            .When(x => !string.IsNullOrEmpty(x.Code))
            .WithMessage("Код должен состоять из 5 символов");
    }
}