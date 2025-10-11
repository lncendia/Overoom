using Films.Infrastructure.Web.Rooms.InputModels;
using FluentValidation;

namespace Films.Infrastructure.Web.Rooms.Validators;

/// <summary>
/// Fluent-валидатор для <see cref="CreateRoomInputModel"/>
/// </summary>
public class CreateRoomValidator : AbstractValidator<CreateRoomInputModel>
{
    /// <summary>
    /// Инициализирует правила валидации для создания комнаты
    /// </summary>
    public CreateRoomValidator()
    {
        RuleFor(x => x.FilmId)
            .NotEmpty()
            .WithMessage("Поле не должно быть пустым");
    }
}