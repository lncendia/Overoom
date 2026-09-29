using Common.Domain.Rooms;
using Common.IntegrationEvents.Rooms;

namespace Films.Application.Services.EventHandlers;

/// <summary>
/// Преобразование настроек комнат в контракт интеграционных событий
/// </summary>
internal static class RoomSettingsContractExtensions
{
  /// <summary>
  /// Преобразует настройки зрителя в контракт интеграционного события
  /// </summary>
  /// <param name="settings">Настройки зрителя</param>
  /// <returns>Настройки в составе интеграционного события</returns>
  public static RoomSettingsData ToContract(this RoomSettings settings)
  {
    return new RoomSettingsData
    {
      Beep = settings.Beep,
      Screamer = settings.Screamer
    };
  }
}
