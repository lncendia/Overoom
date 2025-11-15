using Common.Application.Events;
using Common.Domain.Events;
using Rooms.Application.Abstractions.RoomEvents.Notifications;
using Rooms.Application.Abstractions.Services;
using Rooms.Domain.Rooms;

namespace Rooms.Application.Services.EventHandlers.Rooms;

/// <summary>
/// Обработчик события удаления комнаты
/// </summary>
/// <param name="eventSender">Отправитель событий комнаты</param>
public class RoomDeletedEventHandler(IRoomEventSender eventSender) : AfterSaveNotificationHandler<DeleteEvent<Room>>
{
    /// <summary>
    /// Обрабатывает событие удаления комнаты
    /// </summary>
    /// <param name="event">Событие удаления комнаты</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(DeleteEvent<Room> @event, CancellationToken cancellationToken)
    {
        await eventSender.SendAsync(new DeleteNotificationEvent(), @event.Id, null, cancellationToken);
    }
}