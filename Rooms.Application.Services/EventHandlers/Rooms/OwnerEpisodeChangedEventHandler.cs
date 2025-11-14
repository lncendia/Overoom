using Common.Application.Events;
using Common.Application.ScopedDictionary;
using Rooms.Application.Abstractions;
using Rooms.Application.Abstractions.RoomEvents.Player;
using Rooms.Application.Abstractions.Services;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Rooms;

/// <summary>
/// Обработчик события изменения серии владельцем комнаты
/// </summary>
/// <param name="eventSender">Отправитель событий комнаты</param>
/// <param name="context">Контекст выполняемой области</param>
public class OwnerEpisodeChangedEventHandler(IRoomEventSender eventSender, IScopedContext context)
    : AfterSaveNotificationHandler<ViewerEpisodeChangedEvent>
{
    /// <summary>
    /// Обрабатывает событие изменения серии
    /// </summary>
    /// <param name="event">Событие изменения серии</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(ViewerEpisodeChangedEvent @event, CancellationToken cancellationToken)
    {
        if (@event.Viewer != @event.Room.Owner) return;

        var excludedConnectionId = context.Current.Get<string>(Constants.ScopedDictionary.CurrentConnectionIdKey);

        await eventSender.SendAsync(new EpisodeEvent
        {
            Season = @event.Viewer.Season!.Value,
            Episode = @event.Viewer.Episode!.Value
        }, @event.Room.Id, excludedConnectionId, cancellationToken);
    }
}