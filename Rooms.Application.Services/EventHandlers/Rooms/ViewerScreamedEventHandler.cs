using Common.Application.Events;
using Rooms.Application.Abstractions.RoomEvents.Notifications;
using Rooms.Application.Abstractions.Services;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Rooms;

/// <summary>
/// Обработчик события отправки крика зрителем
/// </summary>
/// <param name="eventSender">Отправитель событий комнаты</param>
public class ViewerScreamedEventHandler(IRoomEventSender eventSender) : AfterSaveNotificationHandler<ViewerScreamedEvent>
{
    /// <summary>
    /// Обрабатывает событие отправки крика зрителем
    /// </summary>
    /// <param name="event">Событие отправки крика</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(ViewerScreamedEvent @event, CancellationToken cancellationToken)
    {
        await eventSender.SendAsync(new ScreamNotificationEvent
        {
            Initiator = @event.Initiator.Id,
            Target = @event.Target.Id
        }, @event.Room.Id, null, cancellationToken);
    }
}