namespace Rooms.Application.Abstractions.Commands;

/// <summary>
/// Команда на изменение номера сезона и серии
/// </summary>
public class SetEpisodeCommand : RoomBaseCommand
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