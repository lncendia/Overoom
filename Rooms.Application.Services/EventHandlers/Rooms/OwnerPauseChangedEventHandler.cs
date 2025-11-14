using Common.Application.Events;
using Common.Application.ScopedDictionary;
using Rooms.Application.Abstractions;
using Rooms.Application.Abstractions.RoomEvents.Player;
using Rooms.Application.Abstractions.Services;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Rooms;

/// <summary>
/// Обработчик события изменения состояния паузы владельцем комнаты
/// </summary>
/// <param name="eventSender">Отправитель событий комнаты</param>
/// <param name="context">Контекст выполняемой области</param>
public class OwnerPauseChangedEventHandler(IRoomEventSender eventSender, IScopedContext context)
    : AfterSaveNotificationHandler<ViewerPauseChangedEvent>
{
    /// <summary>
    /// Обрабатывает событие изменения состояния паузы
    /// </summary>
    /// <param name="event">Событие изменения паузы</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(ViewerPauseChangedEvent @event, CancellationToken cancellationToken)
    {
        if (@event.Viewer != @event.Room.Owner) return;

        var excludedConnectionId = context.Current.Get<string>(Constants.ScopedDictionary.CurrentConnectionIdKey);

        await eventSender.SendAsync(new PauseEvent { Pause = @event.Viewer.OnPause }, @event.Room.Id, excludedConnectionId,
            cancellationToken);
    }
}