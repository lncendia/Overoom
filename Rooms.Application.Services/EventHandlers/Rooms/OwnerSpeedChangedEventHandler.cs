using Common.Application.Events;
using Common.Application.ScopedDictionary;
using Rooms.Application.Abstractions;
using Rooms.Application.Abstractions.RoomEvents.Player;
using Rooms.Application.Abstractions.Services;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Rooms;

/// <summary>
/// Обработчик события изменения скорости воспроизведения владельцем комнаты
/// </summary>
/// <param name="eventSender">Отправитель событий комнаты</param>
/// <param name="context">Контекст выполняемой области</param>
public class OwnerSpeedChangedEventHandler(IRoomEventSender eventSender, IScopedContext context)
    : AfterSaveNotificationHandler<ViewerSpeedChangedEvent>
{
    /// <summary>
    /// Обрабатывает событие изменения скорости воспроизведения
    /// </summary>
    /// <param name="event">Событие изменения скорости</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(ViewerSpeedChangedEvent @event, CancellationToken cancellationToken)
    {
        if (@event.Viewer != @event.Room.Owner) return;

        var excludedConnectionId = context.Current.Get<string>(Constants.ScopedDictionary.CurrentConnectionIdKey);

        await eventSender.SendAsync(new SpeedEvent { Speed = @event.Viewer.Speed }, @event.Room.Id, excludedConnectionId, cancellationToken);
    }
}