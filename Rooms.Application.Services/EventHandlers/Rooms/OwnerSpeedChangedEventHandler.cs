using Common.Application.Events;
using Common.Application.ScopedDictionary;
using MassTransit;
using Rooms.Application.Abstractions;
using Rooms.Application.Abstractions.Events;
using Rooms.Application.Abstractions.Events.Player;
using Rooms.Domain.Rooms.Events;

namespace Rooms.Application.Services.EventHandlers.Rooms;

/// <summary>
/// Обработчик события изменения скорости воспроизведения владельцем комнаты
/// </summary>
/// <param name="publish">Интерфейс для публикации сообщений</param>
/// <param name="context">Контекст выполняемой области</param>
public class OwnerSpeedChangedEventHandler(IPublishEndpoint publish, IScopedContext context)
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
        
        var connectionId = context.Current.Get<string>(Constants.ScopedDictionary.CurrentConnectionIdKey);
        
        using var _ = context.CreateScope();
        
        context.Current.SetRoomHeaders(@event, connectionId);
        
        await publish.Publish<RoomBaseEvent>(new SpeedEvent { Speed = @event.Viewer.Speed }, cancellationToken);
    }
}