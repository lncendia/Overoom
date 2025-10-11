using Rooms.Application.Abstractions.DTOs;

namespace Rooms.Application.Abstractions.Events.Room;

/// <summary>
/// Модель данных для события получения данных комнаты
/// </summary>
public class RoomEvent : RoomBaseEvent
{
    /// <summary>
    /// Данные комнаты
    /// </summary>
    public required RoomDto Room { get; init; }
}