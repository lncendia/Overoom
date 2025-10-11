using MediatR;

namespace Films.Application.Abstractions.Commands.Rooms;

/// <summary>
/// Команда на блокировку пользователя в комнате с фильмом
/// </summary>
public class KickViewerCommand : IRequest
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required Guid UserId { get; init; }
    
    /// <summary>
    /// Идентификатор пользователя, которого необходимо выгнать
    /// </summary>
    public required Guid TargetId { get; init; }
    
    /// <summary>
    /// Идентификатор комнаты
    /// </summary>
    public required Guid RoomId { get; init; }
}