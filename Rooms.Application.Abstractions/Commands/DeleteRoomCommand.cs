using MediatR;

namespace Rooms.Application.Abstractions.Commands;

/// <summary>
/// Команда на удаление комнаты
/// </summary>
public class DeleteRoomCommand : IRequest
{
    /// <summary>
    /// Идентификатор комнаты
    /// </summary>
    public required Guid RoomId { get; init; }
}