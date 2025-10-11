using MediatR;

namespace Films.Application.Abstractions.Commands.Rooms;

/// <summary>
/// Команда на удаление комнаты с фильмом
/// </summary>
public class DeleteRoomCommand : IRequest
{
    /// <summary>
    /// Идентификатор комнаты
    /// </summary>
    public required Guid RoomId { get; init; }
    
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required Guid UserId { get; set; }
}