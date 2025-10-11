using MediatR;
using Rooms.Application.Abstractions.DTOs;
using Rooms.Application.Abstractions.Events.Player;

namespace Rooms.Application.Abstractions.Queries;

/// <summary>
/// Запрос на получение синхронизированных данных комнаты для восстановления состояния просмотра
/// </summary>
public class GetRoomSyncDataQuery : IRequest<RoomSyncDto>
{
    /// <summary>
    /// Идентификатор комнаты
    /// </summary>
    public required Guid Id { get; init; }
    
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required Guid ViewerId { get; init; }
}