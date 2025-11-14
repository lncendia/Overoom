using Common.Application.Events;
using Rooms.Application.Abstractions.DTOs;
using Rooms.Application.Abstractions.RoomEvents.Messages;
using Rooms.Application.Abstractions.Services;
using Rooms.Domain.Messages.Events;

namespace Rooms.Application.Services.EventHandlers.Rooms;

/// <summary>
/// Обработчик события отправки нового сообщения в чате
/// </summary>
/// <param name="eventSender">Отправитель событий комнаты</param>
public class NewMessageEventHandler(IRoomEventSender eventSender) : AfterSaveNotificationHandler<NewMessageEvent>
{
    /// <summary>
    /// Обрабатывает событие отправки нового сообщения
    /// </summary>
    /// <param name="event">Событие нового сообщения</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(NewMessageEvent @event, CancellationToken cancellationToken)
    {
        // Создаем DTO сообщения
        var dto = new MessageDto
        {
            Id = @event.Message.Id,
            UserId = @event.Message.UserId,
            Text = @event.Message.Text,
            SentAt = @event.Message.SentAt
        };
        
        // Публикуем событие комнаты
        await eventSender.SendAsync(new MessageEvent { Message = dto }, @event.Room.Id, null, cancellationToken);
    }
}