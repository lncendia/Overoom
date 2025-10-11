using Films.Application.Abstractions.DTOs.Rooms;
using MediatR;

namespace Films.Application.Abstractions.Queries.Rooms;

/// <summary>
/// Запрос для получения комнат пользователя
/// </summary>
public class GetUserRoomsQuery : IRequest<IReadOnlyList<RoomShortDto>>
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required Guid UserId { get; init; }
}