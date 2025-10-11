namespace Rooms.Application.Abstractions.Events.Notifications;

/// <summary>
/// Базовое событие уведомления, направленное на конкретного пользователя.
/// </summary>
public abstract class TargetedNotificationEvent : NotificationEvent
{
    /// <summary>
    /// Идентификатор пользователя, на которого направлено событие.
    /// </summary>
    public required Guid Target { get; init; }
}