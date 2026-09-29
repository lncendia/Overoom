using Common.Domain.Rooms;
using Common.IntegrationEvents.Rooms;

namespace Rooms.Infrastructure.Bus.Extensions;

/// <summary>
/// Преобразование настроек комнат из контракта интеграционных событий
/// </summary>
internal static class RoomSettingsContractExtensions
{
  /// <summary>
  /// Преобразует настройки из интеграционного события в настройки зрителя
  /// </summary>
  /// <param name="settings">Настройки в составе интеграционного события</param>
  /// <returns>Настройки зрителя</returns>
  public static RoomSettings ToDomain(this RoomSettingsData settings)
  {
    return new RoomSettings
    {
      Beep = settings.Beep,
      Screamer = settings.Screamer
    };
  }
}
