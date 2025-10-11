namespace Rooms.Application.Abstractions.Commands;

/// <summary>
/// Команда для установки уровня громкости или режима mute
/// </summary>
public class SetVolumeCommand : RoomBaseCommand
{
    /// <summary>
    /// Флаг, указывающий включен ли режим без звука (mute)
    /// </summary>
    public required bool Muted { get; init; }
}