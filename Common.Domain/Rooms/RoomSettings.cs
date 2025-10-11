namespace Common.Domain.Rooms;

/// <summary>
/// Набор настроек комнаты, определяющих разрешенные действия пользователей.
/// </summary>
public record RoomSettings
{
    /// <summary>
    /// Разрешение на воспроизведение звукового сигнала (бип).
    /// </summary>
    public required bool Beep { get; init; }

    /// <summary>
    /// Разрешение на использование громких звуков (криков).
    /// </summary>
    public required bool Screamer { get; init; }
}