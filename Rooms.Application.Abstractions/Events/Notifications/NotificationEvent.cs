
namespace Rooms.Application.Abstractions.Events.Notifications;

/// <summary>
/// Базовое событие уведомления в комнате.
/// Содержит информацию об инициаторе действия.
/// </summary>
public abstract class NotificationEvent : RoomBaseEvent
{
    /// <summary>
    /// Идентификатор пользователя, который инициировал событие.
    /// </summary>
    public required Guid Initiator { get; init; }
}