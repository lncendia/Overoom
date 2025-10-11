namespace Rooms.Application.Abstractions.Events.Player;

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