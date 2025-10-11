namespace Rooms.Application.Abstractions.Commands;

/// <summary>
/// Команда для установки скорости воспроизведения
/// </summary>
public class SetSpeedCommand : RoomBaseCommand
{
    /// <summary>
    /// Скорость воспроизведения (1.0 - нормальная скорость)
    /// </summary>
    public required double Speed { get; init; }
}