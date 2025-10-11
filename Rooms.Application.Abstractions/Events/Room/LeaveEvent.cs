namespace Rooms.Application.Abstractions.Events.Room;

/// <summary>
/// Модель данных для события выхода пользователя из комнаты
/// </summary>
public class LeaveEvent : RoomBaseEvent
{
    /// <summary>
    /// Идентификатор пользователя, который вышел
    /// </summary>
    public required Guid Viewer { get; init; }
}