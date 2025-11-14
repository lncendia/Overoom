using Common.Application.Events;
using Rooms.Application.Abstractions.RoomEvents.Notifications;
using Rooms.Application.Abstractions.RoomEvents.Room;
using Rooms.Application.Abstractions.Services;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Rooms;

/// <summary>
/// Обработчик события отключения зрителя от комнаты
/// </summary>
/// <param name="eventSender">Отправитель событий комнаты</param>
public class ViewerLeavedEventHandler(IRoomEventSender eventSender) : AfterSaveNotificationHandler<ViewerLeavedEvent>
{
    /// <summary>
    /// Обрабатывает событие отключения зрителя
    /// </summary>
    /// <param name="event">Событие отключения зрителя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(ViewerLeavedEvent @event, CancellationToken cancellationToken)
    {
        await eventSender.SendAsync(new LeaveEvent { Viewer = @event.Viewer.Id }, @event.Room.Id, null, cancellationToken);
        
        await eventSender.SendAsync(new LeaveNotificationEvent
        {
            Initiator = @event.Viewer.Id,
            Name = @event.Viewer.UserName
        }, @event.Room.Id, null, cancellationToken);
    }
}