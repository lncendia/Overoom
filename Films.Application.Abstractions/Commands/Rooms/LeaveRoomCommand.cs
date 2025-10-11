using MediatR;

namespace Films.Application.Abstractions.Commands.Rooms;

/// <summary>
/// Команда на отключение от комнаты с фильмом
/// </summary>
public class LeaveRoomCommand : IRequest
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Идентификатор комнаты
    /// </summary>
    public required Guid RoomId { get; init; }
}