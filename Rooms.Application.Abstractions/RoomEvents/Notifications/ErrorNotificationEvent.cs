namespace Rooms.Application.Abstractions.RoomEvents.Notifications;

/// <summary>
/// Событие для отправки уведомления об ошибке в комнате.
/// </summary>
public class ErrorNotificationEvent : RoomBaseEvent
{
    /// <summary>
    /// Текст сообщения об ошибке.
    /// </summary>
    public required string Message { get; init; }
}
