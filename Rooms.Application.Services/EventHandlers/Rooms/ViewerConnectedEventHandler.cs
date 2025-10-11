using Common.Application.Events;
using Common.Application.ScopedDictionary;
using MassTransit;
using Rooms.Application.Abstractions.DTOs;
using Rooms.Application.Abstractions.Events;
using Rooms.Application.Abstractions.Events.Notifications;
using Rooms.Application.Abstractions.Events.Room;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Rooms;

/// <summary>
/// Обработчик события подключения зрителя к комнате
/// </summary>
/// <param name="publish">Интерфейс для публикации сообщений</param>
/// <param name="context">Контекст выполняемой области</param>
public class ViewerConnectedEventHandler(IPublishEndpoint publish, IScopedContext context)
    : AfterSaveNotificationHandler<ViewerJoinedEvent>
{
    /// <summary>
    /// Обрабатывает событие подключения зрителя
    /// </summary>
    /// <param name="event">Событие подключения зрителя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(ViewerJoinedEvent @event, CancellationToken cancellationToken)
    {
        using var _ = context.CreateScope();

        context.Current.SetRoomHeaders(@event);

        await publish.Publish<RoomBaseEvent>(new JoinEvent { Viewer = ViewerDto.Create(@event.Viewer) },
            cancellationToken);

        await publish.Publish<RoomBaseEvent>(
            new JoinNotificationEvent { Initiator = @event.Viewer.Id, Name = @event.Viewer.UserName },
            cancellationToken);
    }
}