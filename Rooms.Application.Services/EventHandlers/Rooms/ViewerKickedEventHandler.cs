using Common.Application.Events;
using Common.Application.ScopedDictionary;
using MassTransit;
using Rooms.Application.Abstractions.Events;
using Rooms.Application.Abstractions.Events.Notifications;
using Rooms.Application.Abstractions.Events.Room;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Rooms;

/// <summary>
/// Обработчик события исключения зрителя из комнаты
/// </summary>
/// <param name="publish">Интерфейс для публикации сообщений</param>
/// <param name="context">Контекст выполняемой области</param>
public class ViewerKickedEventHandler(IPublishEndpoint publish, IScopedContext context) : AfterSaveNotificationHandler<ViewerKickedEvent>
{
    /// <summary>
    /// Обрабатывает событие исключения зрителя
    /// </summary>
    /// <param name="event">Событие исключения зрителя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(ViewerKickedEvent @event, CancellationToken cancellationToken)
    {
        using var _ = context.CreateScope();
        
        context.Current.SetRoomHeaders(@event);
        
        await publish.Publish<RoomBaseEvent>(new LeaveEvent { Viewer = @event.Target.Id }, cancellationToken);
        
        await publish.Publish<RoomBaseEvent>(new KickNotificationEvent
        {
            Initiator = @event.Room.Owner.Id,
            Target = @event.Target.Id,
            Name = @event.Target.UserName
        }, cancellationToken);
    }
}