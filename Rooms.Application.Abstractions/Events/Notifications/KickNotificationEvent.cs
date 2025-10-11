namespace Rooms.Application.Abstractions.Events.Notifications;

/// <summary>
/// Событие уведомления об исключении пользователя из комнаты.
/// </summary>
public sealed class KickNotificationEvent : TargetedNotificationEvent
{
    /// <summary>
    /// Имя вышедшего зрителя
    /// </summary>
    public required string Name { get; init; }
}