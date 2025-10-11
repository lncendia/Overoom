namespace Rooms.Application.Abstractions.Events.Player;

/// <summary>
/// Событие изменения скорости воспроизведения медиа-контента в комнате
/// </summary>
public class SpeedEvent : RoomBaseEvent
{
    /// <summary>
    /// Новая скорость воспроизведения медиа-контента
    /// </summary>
    public required double Speed { get; init; }
}