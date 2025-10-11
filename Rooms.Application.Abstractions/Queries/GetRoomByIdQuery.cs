using MediatR;
using Rooms.Application.Abstractions.DTOs;

namespace Rooms.Application.Abstractions.Queries;

/// <summary>
/// Запрос на получение комнаты
/// </summary>
public class GetRoomByIdQuery : IRequest<RoomDto>
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