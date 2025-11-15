using Rooms.Application.Abstractions.DTOs;

namespace Rooms.Application.Abstractions.RoomEvents.Room;

/// <summary>
/// Модель данных для события подключения пользователя
/// </summary>
public class JoinEvent : RoomBaseEvent
{
    /// <summary>
    /// Данные зрителя
    /// </summary>
    public required ViewerDto Viewer { get; init; }
}