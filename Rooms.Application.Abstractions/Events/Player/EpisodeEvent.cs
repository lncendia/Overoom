namespace Rooms.Application.Abstractions.Events.Player;

/// <summary>
/// Событие изменения номера сезона и серии
/// </summary>
public class EpisodeEvent : RoomBaseEvent
{
    /// <summary>
    /// Сезон
    /// </summary>
    public required int Season { get; init; }

    /// <summary>
    /// Серия
    /// </summary>
    public required int Episode { get; init; }
}