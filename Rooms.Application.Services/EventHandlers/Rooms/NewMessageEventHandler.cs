using Common.Application.Events;
using Common.Application.ScopedDictionary;
using MassTransit;
using Rooms.Application.Abstractions.DTOs;
using Rooms.Application.Abstractions.Events;
using Rooms.Application.Abstractions.Events.Messages;
using Rooms.Domain.Messages.Events;

namespace Rooms.Application.Services.EventHandlers.Rooms;

/// <summary>
/// Обработчик события отправки нового сообщения в чате
/// </summary>
/// <param name="publish">Интерфейс для публикации сообщений</param>
/// <param name="context">Контекст выполняемой области</param>
public class NewMessageEventHandler(IPublishEndpoint publish, IScopedContext context) : AfterSaveNotificationHandler<NewMessageEvent>
{
    /// <summary>
    /// Обрабатывает событие отправки нового сообщения
    /// </summary>
    /// <param name="event">Событие нового сообщения</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override async Task Execute(NewMessageEvent @event, CancellationToken cancellationToken)
    {
        using var _ = context.CreateScope();
            
        // Создаем DTO сообщения
        var dto = new MessageDto
        {
            Id = @event.Message.Id,
            UserId = @event.Message.UserId,
            Text = @event.Message.Text,
            SentAt = @event.Message.SentAt
        };
        
        context.Current.SetRoomHeaders(@event);
        
        // Публикуем событие комнаты
        await publish.Publish<RoomBaseEvent>(new MessageEvent { Message = dto }, cancellationToken);
    }
}