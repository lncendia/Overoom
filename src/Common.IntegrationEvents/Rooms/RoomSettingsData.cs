namespace Common.IntegrationEvents.Rooms;

/// <summary>
/// Настройки зрителя в комнатах в составе интеграционных событий.
/// </summary>
/// <remarks>
/// Отдельный тип контракта, чтобы контракты между сервисами не зависели от доменной модели.
/// </remarks>
public record RoomSettingsData
{
  /// <summary>
  /// Разрешены ли звуковые сигналы
  /// </summary>
  public required bool Beep { get; init; }

  /// <summary>
  /// Разрешены ли скримеры
  /// </summary>
  public required bool Screamer { get; init; }
}
