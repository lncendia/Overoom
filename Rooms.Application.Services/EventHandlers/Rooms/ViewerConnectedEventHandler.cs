using Common.Application.Events;
using Rooms.Application.Abstractions.DTOs;
using Rooms.Application.Abstractions.RoomEvents.Notifications;
using Rooms.Application.Abstractions.RoomEvents.Room;
using Rooms.Application.Abstractions.Services;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Rooms;

/// <summary>
/// Обработчик события подключения зрителя к комнате
/// </summary>
/// <param name="eventSender">Отправитель событий комнаты</param>
public class ViewerConnectedEventHandler(IRoomEventSender eventSender) : AfterSaveNotificationHandler<ViewerJoinedEvent>
{
    /// <summary>
    /// Обрабатывает событие подключения зрителя
    /// </summary>
    /// <param name="event">Событие подключения зрителя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(ViewerJoinedEvent @event, CancellationToken cancellationToken)
    {
        await eventSender.SendAsync(new JoinEvent { Viewer = ViewerDto.Create(@event.Viewer) },
            @event.Room.Id, null, cancellationToken);

        await eventSender.SendAsync(
            new JoinNotificationEvent { Initiator = @event.Viewer.Id, Name = @event.Viewer.UserName },
            @event.Room.Id, null, cancellationToken);
    }
}