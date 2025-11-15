using Common.Application.Events;
using Common.Domain.Events;
using MassTransit;
using Rooms.Application.Abstractions.Services;
using Rooms.Domain.Rooms;

namespace Rooms.Application.Services.EventHandlers.Rooms;

/// <summary>
/// Обработчик события удаления комнаты, выполняющий очистку связанных сообщений
/// </summary>
/// <param name="publishEndpoint">Сервис для публикации событий.</param>
public class DeleteMessagesEventHandler(IPublishEndpoint publishEndpoint)
    : BeforeSaveNotificationHandler<DeleteEvent<Room>>
{
    /// <summary>
    /// Обрабатывает событие удаления комнаты
    /// </summary>
    /// <param name="event">Событие удаления комнаты</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    protected override Task Execute(DeleteEvent<Room> @event, CancellationToken cancellationToken)
    {
        // Помещаем задачу очистки сообщений в очередь.
        // Это предотвращает блокировку основного потока при удалении большого количества сообщений
        return publishEndpoint.Publish(new CleanMessages(@event.Id), cancellationToken);
    }
}