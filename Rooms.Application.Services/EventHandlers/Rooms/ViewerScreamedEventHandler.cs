using Common.Application.Events;
using Common.Application.ScopedDictionary;
using MassTransit;
using Rooms.Application.Abstractions.Events;
using Rooms.Application.Abstractions.Events.Notifications;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Rooms;

/// <summary>
/// Обработчик события отправки крика зрителем
/// </summary>
/// <param name="publish">Интерфейс для публикации сообщений</param>
/// <param name="context">Контекст выполняемой области</param>
public class ViewerScreamedEventHandler(IPublishEndpoint publish, IScopedContext context) : AfterSaveNotificationHandler<ViewerScreamedEvent>
{
    /// <summary>
    /// Обрабатывает событие отправки крика зрителем
    /// </summary>
    /// <param name="event">Событие отправки крика</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(ViewerScreamedEvent @event, CancellationToken cancellationToken)
    {
        using var _ = context.CreateScope();
        
        context.Current.SetRoomHeaders(@event);
        
        await publish.Publish<RoomBaseEvent>(new ScreamNotificationEvent
        {
            Initiator = @event.Initiator.Id,
            Target = @event.Target.Id
        }, cancellationToken);
    }
}