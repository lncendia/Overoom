using MediatR;

namespace Films.Application.Abstractions.Commands.Rooms;

/// <summary>
/// Команда на создание комнаты с фильмом
/// </summary>
public class CreateRoomCommand : IRequest<Guid>
{
    /// <summary>
    /// Идентификатор фильма
    /// </summary>
    public required Guid FilmId { get; init; }
    
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required Guid UserId { get; set; }

    /// <summary>
    /// Флаг, открыта ли комната
    /// </summary>
    public required bool IsOpen { get; init; }
}