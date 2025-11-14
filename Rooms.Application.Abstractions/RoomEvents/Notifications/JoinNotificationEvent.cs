namespace Rooms.Application.Abstractions.RoomEvents.Notifications;

/// <summary>
/// Событие уведомления о подключении пользователя к комнате.
/// </summary>
public sealed class JoinNotificationEvent : NotificationEvent
{
    /// <summary>
    /// Имя подключившегося зрителя
    /// </summary>
    public required string Name { get; init; }
}