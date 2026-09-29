using Common.Application.Events;
using Rooms.Application.Abstractions.RoomEvents.Messages;
using Rooms.Application.Abstractions.Services;
using Rooms.Domain.Messages.Events;

namespace Rooms.Application.Services.EventHandlers.Rooms;

/// <summary>
/// Обработчик события установки или снятия реакции на сообщение, уведомляющий зрителей комнаты
/// </summary>
/// <param name="eventSender">Отправитель событий комнаты</param>
public class MessageReactionToggledEventHandler(IRoomEventSender eventSender)
  : AfterSaveNotificationHandler<MessageReactionToggledEvent>
{
  /// <summary>
  /// Обрабатывает событие установки или снятия реакции
  /// </summary>
  /// <param name="event">Событие реакции на сообщение</param>
  /// <param name="cancellationToken">Токен отмены операции</param>
  protected override Task Execute(MessageReactionToggledEvent @event, CancellationToken cancellationToken)
  {
    return eventSender.SendAsync(new MessageReactionEvent
    {
      MessageId = @event.Message.Id,
      ViewerId = @event.ViewerId,
      Reaction = @event.Reaction,
      Added = @event.Added
    }, @event.Room.Id, null, cancellationToken);
  }
}
