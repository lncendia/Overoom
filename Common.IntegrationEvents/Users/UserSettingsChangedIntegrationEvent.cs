using Common.Domain.Rooms;

namespace Common.IntegrationEvents.Users;

/// <summary>
/// Событие интеграции, которое генерируется при изменении данных пользователя.
/// </summary>
public class UserSettingsChangedIntegrationEvent
{
    /// <summary>
    /// Идентификатор пользователя, данные которого были изменены.
    /// </summary>
    public required Guid Id { get; init; }
    
    /// <summary>
    /// Настройки пользователя.
    /// </summary>
    public required RoomSettings Settings { get; init; }
}