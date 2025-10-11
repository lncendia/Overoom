using Common.Application.Events;
using Common.Application.ScopedDictionary;
using Common.Domain.Events;
using MassTransit;
using Rooms.Application.Abstractions.Events;
using Rooms.Application.Abstractions.Events.Notifications;
using Rooms.Domain.Rooms;

namespace Rooms.Application.Services.EventHandlers.Rooms;

/// <summary>
/// Обработчик события удаления комнаты
/// </summary>
/// <param name="publish">Интерфейс для публикации сообщений</param>
/// <param name="context">Контекст выполняемой области</param>
public class RoomDeletedEventHandler(IPublishEndpoint publish, IScopedContext context) : AfterSaveNotificationHandler<DeleteEvent<Room>>
{
    /// <summary>
    /// Обрабатывает событие удаления комнаты
    /// </summary>
    /// <param name="event">Событие удаления комнаты</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(DeleteEvent<Room> @event, CancellationToken cancellationToken)
    {
        using var _ = context.CreateScope();
        
        context.Current.SetRoomHeaders(@event.Id);
        
        await publish.Publish<RoomBaseEvent>(new DeleteNotificationEvent(), cancellationToken);
    }
}