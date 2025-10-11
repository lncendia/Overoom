using Common.Application.Events;
using Common.Application.ScopedDictionary;
using MassTransit;
using Rooms.Application.Abstractions.Events;
using Rooms.Application.Abstractions.Events.Notifications;
using Rooms.Application.Abstractions.Events.Room;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Rooms;

/// <summary>
/// Обработчик события отключения зрителя от комнаты
/// </summary>
/// <param name="publish">Интерфейс для публикации сообщений</param>
/// <param name="context">Контекст выполняемой области</param>
public class ViewerLeavedEventHandler(IPublishEndpoint publish, IScopedContext context)
    : AfterSaveNotificationHandler<ViewerLeavedEvent>
{
    /// <summary>
    /// Обрабатывает событие отключения зрителя
    /// </summary>
    /// <param name="event">Событие отключения зрителя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(ViewerLeavedEvent @event, CancellationToken cancellationToken)
    {
        using var _ = context.CreateScope();
        
        context.Current.SetRoomHeaders(@event);
        
        await publish.Publish<RoomBaseEvent>(new LeaveEvent { Viewer = @event.Viewer.Id }, cancellationToken);
        
        await publish.Publish<RoomBaseEvent>(new LeaveNotificationEvent
        {
            Initiator = @event.Viewer.Id,
            Name = @event.Viewer.UserName
        }, cancellationToken);
    }
}