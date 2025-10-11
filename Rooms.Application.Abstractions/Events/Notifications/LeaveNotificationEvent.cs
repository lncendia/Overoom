namespace Rooms.Application.Abstractions.Events.Notifications;

/// <summary>
/// Событие уведомления о выходе пользователя из комнаты.
/// </summary>
public sealed class LeaveNotificationEvent : NotificationEvent
{
    /// <summary>
    /// Имя вышедшего зрителя
    /// </summary>
    public required string Name { get; init; }
}