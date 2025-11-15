using Common.Application.Events;
using Rooms.Application.Abstractions.RoomEvents.Notifications;
using Rooms.Application.Abstractions.Services;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Rooms;

/// <summary>
/// Обработчик события отправки бипа (звукового сигнала) зрителем
/// </summary>
/// <param name="eventSender">Отправитель событий комнаты</param>
public class ViewerBeepedEventHandler(IRoomEventSender eventSender) : AfterSaveNotificationHandler<ViewerBeepedEvent>
{
    /// <summary>
    /// Обрабатывает событие отправки бипа зрителем
    /// </summary>
    /// <param name="event">Событие отправки бипа</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(ViewerBeepedEvent @event, CancellationToken cancellationToken)
    {
        await eventSender.SendAsync(new BeepNotificationEvent
        {
            Initiator = @event.Initiator.Id,
            Target = @event.Target.Id
        }, @event.Room.Id, null, cancellationToken);
    }
}