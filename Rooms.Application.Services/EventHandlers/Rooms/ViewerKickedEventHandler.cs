using Common.Application.Events;
using Rooms.Application.Abstractions.RoomEvents.Notifications;
using Rooms.Application.Abstractions.RoomEvents.Room;
using Rooms.Application.Abstractions.Services;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Rooms;

/// <summary>
/// Обработчик события исключения зрителя из комнаты
/// </summary>
/// <param name="eventSender">Отправитель событий комнаты</param>
public class ViewerKickedEventHandler(IRoomEventSender eventSender) : AfterSaveNotificationHandler<ViewerKickedEvent>
{
    /// <summary>
    /// Обрабатывает событие исключения зрителя
    /// </summary>
    /// <param name="event">Событие исключения зрителя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(ViewerKickedEvent @event, CancellationToken cancellationToken)
    {
        await eventSender.SendAsync(new LeaveEvent { Viewer = @event.Target.Id }, @event.Room.Id, null,
            cancellationToken);

        await eventSender.SendAsync(new KickNotificationEvent
        {
            Initiator = @event.Room.Owner.Id,
            Target = @event.Target.Id,
            Name = @event.Target.UserName
        }, @event.Room.Id, null, cancellationToken);
    }
}