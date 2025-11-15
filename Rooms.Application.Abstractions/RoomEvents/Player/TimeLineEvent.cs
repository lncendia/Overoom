namespace Rooms.Application.Abstractions.RoomEvents.Player;

/// <summary>
/// Событие установки таймлайна
/// </summary>
public class TimeLineEvent : RoomBaseEvent
{
    /// <summary>
    /// Таймлайн
    /// </summary>
    public required long TimeLine { get; init; }
}