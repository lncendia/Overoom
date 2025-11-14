using Common.Application.Events;
using Common.Application.ScopedDictionary;
using Rooms.Application.Abstractions;
using Rooms.Application.Abstractions.RoomEvents.Player;
using Rooms.Application.Abstractions.Services;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Rooms;

/// <summary>
/// Обработчик события изменения временной позиции владельцем комнаты
/// </summary>
/// <param name="eventSender">Отправитель событий комнаты</param>
/// <param name="context">Контекст выполняемой области</param>
public class OwnerTimeLineChangedEventHandler(IRoomEventSender eventSender, IScopedContext context)
    : AfterSaveNotificationHandler<ViewerTimeLineChangedEvent>
{
    /// <summary>
    /// Обрабатывает событие изменения временной позиции
    /// </summary>
    /// <param name="event">Событие изменения временной позиции</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(ViewerTimeLineChangedEvent @event, CancellationToken cancellationToken)
    {
        if (@event.Viewer != @event.Room.Owner) return;

        var excludedConnectionId = context.Current.Get<string>(Constants.ScopedDictionary.CurrentConnectionIdKey);

        await eventSender.SendAsync(new TimeLineEvent { TimeLine = @event.Viewer.TimeLine.Ticks }, @event.Room.Id,
            excludedConnectionId, cancellationToken);
    }
}