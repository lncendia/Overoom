namespace Rooms.Application.Abstractions.RoomEvents.Player;

/// <summary>
/// Событие установки паузы
/// </summary>
public class PauseEvent : RoomBaseEvent
{
    /// <summary>
    /// Флаг нахождения на паузе
    /// </summary>
    public required bool Pause { get; init; }
}