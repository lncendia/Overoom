using MediatR;
using Rooms.Application.Abstractions.DTOs;

namespace Rooms.Application.Abstractions.Commands;

/// <summary>
/// Команда на подключение к комнате
/// </summary>
public class JoinCommand : IRequest<RoomDto>
{
    /// <summary>
    /// Зритель
    /// </summary>
    public required ViewerData Viewer { get; init; }

    /// <summary>
    /// Идентификатор комнаты
    /// </summary>
    public required Guid RoomId { get; init; }
}