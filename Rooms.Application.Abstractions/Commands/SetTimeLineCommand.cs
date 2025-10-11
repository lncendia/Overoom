namespace Rooms.Application.Abstractions.Commands;

/// <summary>
/// Команда на установку таймлайна
/// </summary>
public class SetTimeLineCommand : RoomBaseCommand
{
    /// <summary>
    /// Таймлайн
    /// </summary>
    public required TimeSpan TimeLine { get; init; }
}