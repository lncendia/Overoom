namespace Rooms.Application.Abstractions.Commands;

/// <summary>
/// Команда на установку режима полного экрана
/// </summary>
public class SetFullscreenCommand : RoomBaseCommand
{
    /// <summary>
    /// Флаг нахождения в полноэкранном режиме
    /// </summary>
    public required bool Fullscreen { get; init; }
}