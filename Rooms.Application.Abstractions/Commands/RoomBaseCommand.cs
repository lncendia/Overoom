using MediatR;

namespace Rooms.Application.Abstractions.Commands;

/// <summary>
/// Базовый класс для команды комнаты
/// </summary>
public abstract class RoomBaseCommand : IRequest
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required Guid ViewerId { get; init; }

    /// <summary>
    /// Идентификатор комнаты
    /// </summary>
    public required Guid RoomId { get; init; }
}